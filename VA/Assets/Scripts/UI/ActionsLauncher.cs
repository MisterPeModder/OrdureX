
using System;
using System.Collections.Generic;
using MQTTnet;
using MQTTnet.Protocol;
using OrdureX.Mqtt;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OrdureX.UI
{
    public class ActionsLauncher : MonoBehaviour
    {
        public OrdureXEvents Events;
        public TMP_Dropdown Dropdown;

        public Toggle StartButton;
        public Button RunActionButton;
        public TMP_InputField ActionInputField;
        public MqttController MqttController;

        private int selectedAction = 0;
        private readonly List<Action> actions = new();
        private readonly List<Action> onSelectActions = new();
        private readonly List<Action> onDeselectActions = new();

        private SimulationStateManager m_SimulationStateManager;

        private const string ACTION_NAMESPACE = "ordurex/action";

        private void Awake()
        {
            m_SimulationStateManager = FindObjectOfType<SimulationStateManager>();
        }

        void Start()
        {
            Dropdown.options.Clear();
            actions.Clear();
            onSelectActions.Clear();
            onDeselectActions.Clear();
            HideActionInputField();
            AddAction("Select an action...", () => { });
            AddAction("Trash 0: open lid", OpenTrash0Lid);
            AddAction("Trash 0: request collect", RequestCollectTrash0, ShowRequestCollectCodeField, HideActionInputField);
            AddAction("Trash 1: ring buzzer", RingTrash1Buzzer);
            AddAction("Trash 1: request collect", RequestCollectTrash1, ShowDisplayTextField, HideActionInputField);
            AddAction("Trash 2: open lid", OpenTrash2Lid);
            AddAction("Trash 2: display text", DisplayTextOnTrash2, ShowDisplayTextField, HideActionInputField);
            AddAction("Trash 2: request collect", RequestCollectTrash2, ShowRequestCollectCodeField, HideActionInputField);
            Dropdown.value = 0;
            selectedAction = 0;
            Dropdown.RefreshShownValue();
        }

        // Update is called once per frame
        void Update()
        {
            var interactable = m_SimulationStateManager.Status == SimulationStatus.Running
                || m_SimulationStateManager.Status == SimulationStatus.Paused
                || m_SimulationStateManager.Status == SimulationStatus.Stopped;
            StartButton.interactable = interactable;
            RunActionButton.interactable = interactable;
            Dropdown.interactable = interactable;
            ActionInputField.interactable = interactable;
            var isOn = !(m_SimulationStateManager.Status == SimulationStatus.Paused || m_SimulationStateManager.Status == SimulationStatus.Stopped);
            StartButton.SetIsOnWithoutNotify(isOn);
            StartButton.GetComponent<PlayPauseButton>().UpdateGraphic(isOn);

            if (ActionInputField.interactable && EventSystem.current.currentSelectedGameObject == ActionInputField.gameObject && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                EventSystem.current.SetSelectedGameObject(null);
                RunSelectedAction();
            }
        }

        public void MqttConnected()
        {
        }

        public void RunSelectedAction()
        {
            actions[selectedAction]();
        }

        public void OnDropdownValueChanged()
        {
            onDeselectActions[selectedAction]();
            selectedAction = Dropdown.value;
            onSelectActions[Dropdown.value]();
        }

        private void AddAction(string name, Action action)
        {
            AddAction(name, action, () => { }, () => { });
        }

        private void AddAction(string name, Action action, Action onSelect, Action onDeselect)
        {
            Dropdown.options.Add(new TMP_Dropdown.OptionData(name));
            actions.Add(action);
            onSelectActions.Add(onSelect);
            onDeselectActions.Add(onDeselect);
        }

        private void OpenTrash0Lid()
        {
            Debug.Log("Opening lid of trash 0");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-0/lid")
                .WithPayload(new byte[1] { 1 }) // 1 = open, 0 = close
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        }

        private void RequestCollectTrash0()
        {
            List<byte> payload = new();

            payload.AddRange(Events.ClientUuid.ToByteArray());
            payload.AddRange(System.Text.Encoding.UTF8.GetBytes(LimitTo256Bytes(ActionInputField.text)));

            Debug.Log("Requesting collection of trash 0");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-0/request-collect")
                .WithPayload(payload.ToArray())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        }

        private void RingTrash1Buzzer()
        {
            Debug.Log("Ringing buzzer of trash 1");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-1/buzzer")
                .WithPayload(new byte[1] { 0 }) // 0 = sound 1, 1 = sound 2, ...
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build());
        }

        private void RequestCollectTrash1()
        {
            List<byte> payload = new();

            payload.AddRange(Events.ClientUuid.ToByteArray());
            payload.AddRange(System.Text.Encoding.UTF8.GetBytes(LimitTo256Bytes(ActionInputField.text)));

            Debug.Log("Requesting collection of trash 1");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-1/request-collect")
                .WithPayload(payload.ToArray())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        }

        private void OpenTrash2Lid()
        {
            Debug.Log("Opening lid of trash 2");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-2/lid")
                .WithPayload(new byte[1] { 1 }) // 1 = open, 0 = close
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        }

        private void DisplayTextOnTrash2()
        {
            Debug.Log("Displaying text on Trash 2, text: " + ActionInputField.text);
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-2/display")
                .WithPayload(ActionInputField.text)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build());
        }

        private void RequestCollectTrash2()
        {
            List<byte> payload = new();

            payload.AddRange(Events.ClientUuid.ToByteArray());
            payload.AddRange(System.Text.Encoding.UTF8.GetBytes(LimitTo256Bytes(ActionInputField.text)));

            Debug.Log("Requesting collection of trash 2");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-2/request-collect")
                .WithPayload(payload.ToArray())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        }

        private void ShowRequestCollectCodeField()
        {
            ActionInputField.text = "";
            ActionInputField.placeholder.gameObject.GetComponent<TextMeshProUGUI>().text = "Secret code...";
            ActionInputField.gameObject.SetActive(true);
        }

        private void ShowDisplayTextField()
        {
            ActionInputField.text = "";
            ActionInputField.placeholder.gameObject.GetComponent<TextMeshProUGUI>().text = "Text to display...";
            ActionInputField.gameObject.SetActive(true);
        }

        private void HideActionInputField()
        {
            ActionInputField.gameObject.SetActive(false);
        }

        private string LimitTo256Bytes(string input)
        {
            int byteCount = 0;
            int i = 0;
            while (i < input.Length)
            {
                int charByteCount = System.Text.Encoding.UTF8.GetByteCount(new char[] { input[i] });
                if (byteCount + charByteCount > 256)
                {
                    break;
                }
                byteCount += charByteCount;
                i++;
            }
            return input.Substring(0, i);
        }
    }
}
