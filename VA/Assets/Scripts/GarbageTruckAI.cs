using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;

namespace OrdureX
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GarbageTruckAI : MonoBehaviour
    {
        [Header("Global Objects")]
        [SerializeField] private OrdureXEvents m_Events;
        [SerializeField] private SettingsManager m_SettingsManager;

        [Header("Truck AI Settings")]
        [SerializeField] private List<Transform> m_Targets;
        public List<Transform> Targets
        {
            get => m_Targets;
            set => m_Targets = value;
        }
        [Tooltip("The AI Agent to control, defaults to this GameObject's NavMeshAgent component.")]
        [SerializeField] private NavMeshAgent m_Agent;

        [Tooltip("The distance at which the truck is considered close enough to a trash can.")]
        [SerializeField] private float m_TrashCollectDistance = 0.2f;
        [SerializeField] private Transform m_CurrentTarget;
        [SerializeField] private GameObject m_PathCornerPrefab;
        [SerializeField] private ScaledProjection m_PathCornerProjectionPrefab;
        [SerializeField] private float m_CollectionTime = 2.0f;

        [Header("Display Settings")]
        [SerializeField] private Transform m_BigOrigin;
        [SerializeField] private Transform m_SmallOrigin;
        [SerializeField] private LineRenderer m_PathLine;

        private readonly Queue<int> m_TargetQueue = new();

        private const int k_MaxCorners = 32;
        private GameObject m_PathCornesParent;
        private readonly Vector3[] m_Corners = new Vector3[k_MaxCorners];
        private readonly GameObject[] m_PathCorners = new GameObject[k_MaxCorners];
        private readonly ScaledProjection[] m_PathCornerProjections = new ScaledProjection[k_MaxCorners];

        private void OnEnable()
        {
            m_Events.OnTrash0CollectRequested += OnTrash0CollectRequested;
            m_Events.OnTrash1CollectRequested += OnTrash1CollectRequested;
            m_Events.OnTrash1BurningChanged += OnTrash1CollectRequested;
            m_Events.OnTrash2CollectRequested += OnTrash2CollectRequested;
        }

        private void OnDisable()
        {
            m_Events.OnTrash0CollectRequested -= OnTrash0CollectRequested;
            m_Events.OnTrash1CollectRequested -= OnTrash1CollectRequested;
            m_Events.OnTrash1BurningChanged += OnTrash1CollectRequested;
            m_Events.OnTrash2CollectRequested -= OnTrash2CollectRequested;
        }

        private void Awake()
        {
            if (m_Events == null)
            {
                m_Events = FindObjectOfType<OrdureXEvents>();
            }
            if (m_SettingsManager == null)
            {
                m_SettingsManager = FindObjectOfType<SettingsManager>();
            }
            m_Agent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(Transform bigOrigin, Transform smallOrigin)
        {
            m_BigOrigin = bigOrigin;
            m_SmallOrigin = smallOrigin;
        }

        private void Start()
        {
            m_PathCornesParent = new GameObject("PathCorners");
            for (int i = 0; i < k_MaxCorners; i++)
            {
                var corner = Instantiate(m_PathCornerPrefab, Vector3.zero, Quaternion.identity);
                corner.transform.SetParent(m_PathCornesParent.transform);
                corner.name = $"Path Corner #{i}";
                corner.SetActive(false);
                m_PathCorners[i] = corner;

                var projection = Instantiate(m_PathCornerProjectionPrefab, Vector3.zero, Quaternion.identity);
                projection.Initialize(m_BigOrigin, m_SmallOrigin, corner.transform);
                projection.transform.SetParent(m_PathCornesParent.transform);
                projection.name = $"Path Corner Projection #{i}";
                projection.gameObject.SetActive(false);
                m_PathCornerProjections[i] = projection;
            }
            StartCoroutine(TruckRoutine());
        }

        private void OnDestroy()
        {
            foreach (var corner in m_PathCorners)
            {
                Destroy(corner);
            }
            Destroy(m_PathCornesParent);
        }

        private void Update()
        {
            if (!m_SettingsManager.ShowTruckPath || !m_Agent.hasPath)
            {
                m_PathLine.enabled = false;
                for (int i = 0; i < k_MaxCorners; i++)
                {
                    m_PathCorners[i].SetActive(false);
                    m_PathCornerProjections[i].gameObject.SetActive(false);
                }
                return;
            }
            m_PathLine.enabled = true;
            int nCorners = m_Agent.path.GetCornersNonAlloc(m_Corners);

            var posOffset = Vector3.up * 0.2f;
            for (int i = 0; i < k_MaxCorners; i++)
            {
                if (i < nCorners)
                {
                    m_PathCorners[i].transform.position = m_Corners[i] + posOffset;
                    m_PathCorners[i].SetActive(true);
                    m_PathCornerProjections[i].gameObject.SetActive(true);
                }
                else
                {
                    m_PathCorners[i].SetActive(false);
                    m_PathCornerProjections[i].gameObject.SetActive(false);
                }
            }

            m_PathLine.positionCount = nCorners;
            for (int i = 0; i < nCorners; i++)
            {
                m_PathLine.SetPosition(i, m_PathCornerProjections[i].transform.position);
            }
        }

        private IEnumerator TruckRoutine()
        {
            while (true)
            {
                while (m_Targets.Count == 0)
                {
                    // no targets, wait for a second
                    yield return new WaitForSeconds(1);
                    // Sort the targets by distance to the truck
                    m_Targets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
                }


                // Find next target
                int targetIndex = 0;
                if (m_TargetQueue.TryDequeue(out targetIndex) && targetIndex < m_Targets.Count)
                {
                    m_CurrentTarget = m_Targets[targetIndex];
                }
                else
                {
                    var previousTarget = m_CurrentTarget;
                    int previousIndex = m_Targets.IndexOf(previousTarget);

                    if (previousIndex == -1)
                    {
                        m_CurrentTarget = m_Targets[0];
                        targetIndex = 0;
                    }
                    else
                    {
                        targetIndex = (previousIndex + 1) % m_Targets.Count;
                        m_CurrentTarget = m_Targets[targetIndex];
                    }
                }


                // Move to the target until we reach it
                while (m_CurrentTarget != null)
                {
                    m_Agent.SetDestination(m_CurrentTarget.position);

                    if (Vector3.Distance(transform.position, m_CurrentTarget.position) <= m_TrashCollectDistance)
                    {
                        break;
                    }
                    yield return null; // Wait for the next frame
                }

                if (m_CurrentTarget != null)
                {
                    m_Agent.isStopped = true;
                    yield return new WaitForSeconds(m_CollectionTime);
                    m_Events.SetCollectRequested(targetIndex, false);
                    m_Agent.isStopped = false;
                }
            }
        }

        private void OnTrash0CollectRequested(bool value)
        {
            if (value)
            {
                m_TargetQueue.Enqueue(0);
            }
        }

        private void OnTrash1CollectRequested(bool value)
        {
            if (value)
            {
                m_TargetQueue.Enqueue(1);
            }
        }

        private void OnTrash2CollectRequested(bool value)
        {
            if (value)
            {
                m_TargetQueue.Enqueue(2);
            }
        }
    }
}
