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
        private string m_BrokerURL = "broker.hivemq.com:8000/mqtt";
        public string BrokerURL
        {
            get => m_BrokerURL;
        }
        [Tooltip("MQTT broker username")]
        [SerializeField]
        private string m_BrokerUsername = "";
        public string BrokerUsername
        {
            get => m_BrokerUsername;
        }
        [Tooltip("MQTT broker password")]
        [SerializeField]
        private string m_BrokerPassword = "";
        public string BrokerPassword
        {
            get => m_BrokerPassword;
        }
        public Action OnConnectToBroker { get; set; }
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
            LoadSettings();
            m_SettingsPanel.SetActive(false);
            m_SimulationStateManager.OnStatusChanged += OnSimulationStatusChanged;
            if (m_BrokerURLInputField != null)
            {
                m_BrokerURLInputField.text = m_BrokerURL;
                m_BrokerURLInputField.onValueChanged.AddListener(OnBrokerURLChanged);
            }
            if (m_BrokerPasswordInputField != null)
            {
                m_BrokerPasswordInputField.text = m_BrokerPassword;
                m_BrokerPasswordInputField.onValueChanged.AddListener(OnBrokerPasswordChanged);
            }
            if (m_BrokerUsernameInputField != null)
            {
                m_BrokerUsernameInputField.text = m_BrokerUsername;
                m_BrokerUsernameInputField.onValueChanged.AddListener(OnBrokerUsernameChanged);
            }
            if (m_ConnectToBrokerButton != null)
            {
                m_ConnectToBrokerButton.onClick.AddListener(OnConnectToBrokerButtonClicked);
            }
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

        private void LoadSettings()
        {
            Debug.Log("Loading settings from PlayerPrefs...");
            m_BrokerURL = PlayerPrefs.GetString("BrokerURL", m_BrokerURL);
            m_BrokerUsername = PlayerPrefs.GetString("BrokerUsername", m_BrokerUsername);
            m_BrokerPassword = PlayerPrefs.GetString("BrokerPassword", m_BrokerPassword);
            m_ShowTileDebugOverlay = PlayerPrefs.GetInt("ShowTileDebugOverlay", m_ShowTileDebugOverlay ? 1 : 0) == 1;
            m_SimulateArduino = PlayerPrefs.GetInt("SimulateArduino", m_SimulateArduino ? 1 : 0) == 1;
            Debug.Log("Sucessfully loaded settings from PlayerPrefs");
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

        private void OnBrokerURLChanged(string value)
        {
            PlayerPrefs.SetString("BrokerURL", value);
            PlayerPrefs.Save();
            m_BrokerURL = value;
        }

        private void OnBrokerUsernameChanged(string value)
        {
            PlayerPrefs.SetString("BrokerUsername", value);
            PlayerPrefs.Save();
            m_BrokerUsername = value;
        }

        private void OnBrokerPasswordChanged(string value)
        {
            PlayerPrefs.SetString("BrokerPassword", value);
            PlayerPrefs.Save();
            m_BrokerPassword = value;
        }

        private void OnConnectToBrokerButtonClicked()
        {
            OnConnectToBroker.Invoke();
        }

        private void OnShowTileOverlayButtonClicked(bool value)
        {
            PlayerPrefs.SetInt("ShowTileDebugOverlay", value ? 1 : 0);
            PlayerPrefs.Save();
            m_ShowTileDebugOverlay = value;
        }

        private void OnSimulateArduinoClicked(bool value)
        {
            PlayerPrefs.SetInt("SimulateArduino", value ? 1 : 0);
            PlayerPrefs.Save();
            m_SimulateArduino = value;
            OnSimulateArduinoChanged.Invoke(value);
        }

    }
}
