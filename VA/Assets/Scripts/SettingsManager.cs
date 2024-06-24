using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrdureX
{
    /// <summary>
    /// Central location for the all the settings in the application.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Settings State")]
        [SerializeField]
        private bool m_ShowTileDebugOverlay = false;
        public bool ShowTileDebugOverlay
        {
            get => m_ShowTileDebugOverlay;
        }
        [SerializeField]
        private bool m_SimulateArduino = false;
        public bool SimulateArduino
        {
            get => m_SimulateArduino;
        }
        public Action<bool> OnSimulateArduinoChanged;
        [SerializeField]
        private string m_ServerURL = "broker.hivemq.com:8000/mqtt";
        public string ServerURL
        {
            get => m_ServerURL;
        }
        [Tooltip("MQTT broker username")]
        [SerializeField]
        private string m_Username = null;
        public string Username
        {
            get => m_Username;
        }
        [Tooltip("MQTT broker password")]
        [SerializeField]
        private string m_Password = null;
        public string Password
        {
            get => m_Password;
        }
        public Action OnConnectToBroker;

        [Header("UI Elements")]
        [SerializeField]
        private GameObject m_SettingsPanel;
        [SerializeField]
        private Toggle m_TileDebugOverlayToggle;
        [SerializeField]
        private Toggle m_SimulateArduinoToggle;
        [SerializeField]
        private TMP_InputField m_BrokerURLInputField;
        [SerializeField]
        private TMP_InputField m_BrokerUsernameInputField;
        [SerializeField]
        private TMP_InputField m_BrokerPasswordInputField;
        [SerializeField]
        private Button m_ConnectToBrokerButton;

        private SimulationStateManager m_SimulationStateManager;

        private void Awake()
        {
            m_SimulationStateManager = FindObjectOfType<SimulationStateManager>();
        }

        private void OnEnable()
        {
            m_SettingsPanel.SetActive(false);
            m_ShowTileDebugOverlay = false;
            m_SimulationStateManager.OnStatusChanged += OnSimulationStatusChanged;
            if (m_TileDebugOverlayToggle != null)
            {
                m_TileDebugOverlayToggle.isOn = m_ShowTileDebugOverlay;
                m_TileDebugOverlayToggle.onValueChanged.AddListener(OnShowTileOverlayButtonClicked);
            }
            if (m_SimulateArduinoToggle != null)
            {
                m_SimulateArduinoToggle.isOn = m_SimulateArduino;
                m_SimulateArduinoToggle.onValueChanged.AddListener(OnSimulateArduinoClicked);
            }
            if (m_BrokerURLInputField != null)
            {
                m_BrokerURLInputField.text = m_ServerURL;
                m_BrokerURLInputField.onValueChanged.AddListener(OnBrokerURLChanged);
            }
            if (m_BrokerPasswordInputField != null)
            {
                m_BrokerPasswordInputField.text = m_Password;
                m_BrokerPasswordInputField.onValueChanged.AddListener(OnBrokerPasswordChanged);
            }
            if (m_BrokerUsernameInputField != null)
            {
                m_BrokerUsernameInputField.text = m_Username;
                m_BrokerUsernameInputField.onValueChanged.AddListener(OnBrokerUsernameChanged);
            }
            if (m_ConnectToBrokerButton != null)
            {
                m_ConnectToBrokerButton.onClick.AddListener(OnConnectToBrokerButtonClicked);
            }
        }

        private void OnDisable()
        {
            m_SimulationStateManager.OnStatusChanged -= OnSimulationStatusChanged;
            if (m_TileDebugOverlayToggle != null)
                m_TileDebugOverlayToggle.onValueChanged.RemoveListener(OnShowTileOverlayButtonClicked);
            if (m_SimulateArduinoToggle != null)
                m_SimulateArduinoToggle.onValueChanged.RemoveListener(OnSimulateArduinoClicked);
            if (m_BrokerURLInputField != null)
                m_BrokerURLInputField.onValueChanged.RemoveListener(OnBrokerURLChanged);
            if (m_BrokerPasswordInputField != null)
                m_BrokerPasswordInputField.onValueChanged.RemoveListener(OnBrokerPasswordChanged);
            if (m_BrokerUsernameInputField != null)
                m_BrokerUsernameInputField.onValueChanged.RemoveListener(OnBrokerUsernameChanged);
            if (m_ConnectToBrokerButton != null)
                m_ConnectToBrokerButton.onClick.RemoveListener(OnConnectToBrokerButtonClicked);
        }

        private void OnSimulationStatusChanged(SimulationStatus prevStatus, SimulationStatus newStatus)
        {
            if (m_ConnectToBrokerButton != null)
                m_ConnectToBrokerButton.interactable = newStatus != SimulationStatus.Connecting;
        }

        public void OnSettingsButtonClicked()
        {
            m_SettingsPanel.SetActive(!m_SettingsPanel.activeSelf);
        }

        private void OnShowTileOverlayButtonClicked(bool value)
        {
            m_ShowTileDebugOverlay = value;
        }

        private void OnSimulateArduinoClicked(bool value)
        {
            m_SimulateArduino = value;
            OnSimulateArduinoChanged.Invoke(value);
        }

        private void OnBrokerURLChanged(string value)
        {
            m_ServerURL = value;
        }

        private void OnBrokerUsernameChanged(string value)
        {
            m_Username = value;
        }

        private void OnBrokerPasswordChanged(string value)
        {
            m_Password = value;
        }

        private void OnConnectToBrokerButtonClicked()
        {
            OnConnectToBroker.Invoke();
        }

    }
}
