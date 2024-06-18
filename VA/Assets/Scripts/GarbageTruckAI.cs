using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

        public List<Transform> Targets
        {
            get => m_Targets;
            set => m_Targets = value;
        }

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            StartCoroutine(TruckRoutine());
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
