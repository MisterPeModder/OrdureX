using System.Data.Common;
using UnityEngine;
using UnityEngine.AI;

namespace OrdureX
{
    public class GarbageTruckAI : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;
        [Tooltip("The AI Agent to control, defaults to this GameObject's NavMeshAgent component.")]
        [SerializeField] private NavMeshAgent m_Agent;

        void Start()
        {
            m_Agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if (m_Target != null)
            {
                m_Agent.SetDestination(m_Target.position);
                Debug.DrawLine(transform.position, m_Target.position, Color.red);
            }
        }
    }
}
