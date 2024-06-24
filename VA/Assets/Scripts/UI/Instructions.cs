using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace OrdureX.UI
{
    public class Instructions : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_InstructionsPanel;
        private SimulationStateManager m_SimulationStateManager;

        private void Awake()
        {
            m_SimulationStateManager = FindObjectOfType<SimulationStateManager>();
        }

        private void Start()
        {
            m_InstructionsPanel.SetActive(false);
        }


        private void OnEnable()
        {
            m_SimulationStateManager.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            m_SimulationStateManager.OnStatusChanged -= OnStatusChanged;
        }

        private void OnStatusChanged(SimulationStatus prevStatus, SimulationStatus newStatus)
        {
            m_InstructionsPanel.SetActive(newStatus == SimulationStatus.Stopped);
        }
    }
}
