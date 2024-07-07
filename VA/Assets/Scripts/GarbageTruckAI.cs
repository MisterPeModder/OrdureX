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
        [SerializeField] private List<Transform> m_Targets;
        [Tooltip("The AI Agent to control, defaults to this GameObject's NavMeshAgent component.")]
        [SerializeField] private NavMeshAgent m_Agent;

        [Tooltip("The distance at which the truck is considered close enough to a trash can.")]
        [SerializeField] private float m_TrashCollectDistance = 1.0f;
        [SerializeField] private Transform m_CurrentTarget;
        [SerializeField] private GameObject m_PathCornerPrefab;
        [SerializeField] private ScaledProjection m_PathCornerProjectionPrefab;


        [SerializeField]
        private Transform m_BigOrigin;
        [SerializeField]
        private Transform m_SmallOrigin;
        [SerializeField]
        private LineRenderer m_PathLine;

        private SettingsManager m_SettingsManager;

        private const int k_MaxCorners = 16;
        private GameObject m_PathCornesParent;
        private readonly Vector3[] m_Corners = new Vector3[k_MaxCorners];
        private readonly GameObject[] m_PathCorners = new GameObject[k_MaxCorners];
        private readonly ScaledProjection[] m_PathCornerProjections = new ScaledProjection[k_MaxCorners];

        public List<Transform> Targets
        {
            get => m_Targets;
            set => m_Targets = value;
        }

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_SettingsManager = FindObjectOfType<SettingsManager>();
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
                for (int i = 0; i < k_MaxCorners; i++)
                {
                    m_PathCorners[i].SetActive(false);
                    m_PathCornerProjections[i].gameObject.SetActive(false);
                }
                return;
            }
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
                var previousTarget = m_CurrentTarget;
                int previousIndex = m_Targets.IndexOf(previousTarget);

                if (previousIndex == -1)
                {
                    m_CurrentTarget = m_Targets[0];
                }
                else
                {
                    m_CurrentTarget = m_Targets[(previousIndex + 1) % m_Targets.Count];
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
                    yield return new WaitForSeconds(2);
                    m_Agent.isStopped = false;
                }
            }
        }
    }
}
