
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

        public TMP_Text StartButtonText;
        public Button StartButton;
        public Button RunActionButton;
        public TMP_InputField ActionInputField;
        public MqttController MqttController;

        private int selectedAction = 0;
        private readonly List<Action> actions = new();
        private readonly List<Action> onSelectActions = new();
        private readonly List<Action> onDeselectActions = new();

        private const string ACTION_NAMESPACE = "ordurex/action";

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
            StartButton.interactable = Events.Connected;
            RunActionButton.interactable = Events.Connected;
            Dropdown.interactable = Events.Connected;
            ActionInputField.interactable = Events.Connected;
            if (Events.Status == SimulationStatus.Paused || Events.Status == SimulationStatus.Stopped)
            {
                StartButtonText.text = "Start";
            }
            else
            {
                StartButtonText.text = "Stop";
            }

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
            Debug.Log("Requesting collection of trash 0");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-0/request-collect")
                .WithPayload(ActionInputField.text)
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
            Debug.Log("Requesting collection of trash 1");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-1/request-collect")
                .WithPayload(ActionInputField.text)
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
            Debug.Log("Requesting collection of trash 2");
            MqttController.Publish(new MqttApplicationMessageBuilder()
                .WithTopic($"{ACTION_NAMESPACE}/trash-2/request-collect")
                .WithPayload(ActionInputField.text)
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
    }
}
