using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrdureX
{
    public class SimulationStateManager : MonoBehaviour
    {
        private SimulationStatus m_Status = SimulationStatus.Initial;
        private int m_SubStatus = 0;

        public Action<SimulationStatus, SimulationStatus> OnStatusChanged;

        public SimulationStatus Status
        {
            get => m_Status;
            set => SetStatus(value, m_SubStatus);
        }

        public int SubStatus
        {
            get => m_SubStatus;
            set => SetStatus(m_Status, value);
        }

        [Header("Display")]
        [SerializeField]
        private TMP_Text m_SimulationStageText;
        [SerializeField]
        private Image m_SimulationStageColor;

        private void Start()
        {
            m_Status = SimulationStatus.Initial;
            m_SubStatus = 0;
            UpdateDisplay();
        }

        public void SetStatus(SimulationStatus status, int subStatus = 0)
        {
            if (Status == status && m_SubStatus == subStatus) return;
            var prevStatus = m_Status;
            m_Status = status;
            m_SubStatus = subStatus;
            UpdateDisplay();
            OnStatusChanged.Invoke(prevStatus, m_Status);
        }

        private void UpdateDisplay()
        {
            m_SimulationStageText.text = m_Status.GetDescription(m_SubStatus);
            m_SimulationStageColor.color = m_Status.GetColor();
        }
    }

    public enum SimulationStatus
    {
        Initial,
        Connecting,
        ConnectionFailed,
        Stopped,
        Running,
        Paused,
    }

    public static class SimulationStageExtensions
    {
        public static Color GetColor(this SimulationStatus status)
        {
            return status switch
            {
                SimulationStatus.Initial => Color.gray,
                SimulationStatus.Connecting => Color.yellow,
                SimulationStatus.ConnectionFailed => Color.red,
                SimulationStatus.Stopped => Color.cyan,
                SimulationStatus.Running => Color.green,
                SimulationStatus.Paused => Color.blue,
                _ => Color.magenta,
            };
        }

        public static string GetDescription(this SimulationStatus status, int subStatus)
        {
            return status switch
            {
                SimulationStatus.Initial => "Initializing...",
                SimulationStatus.Connecting => "Connecting".PadRight(10 + subStatus, '.'),
                SimulationStatus.ConnectionFailed => "Connection Failed",
                SimulationStatus.Stopped => "Ready to start",
                SimulationStatus.Running => "Running",
                SimulationStatus.Paused => "Paused",
                _ => "???",
            };
        }
    }



}
