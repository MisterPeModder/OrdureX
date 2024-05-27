
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

        private int selectedAction = 0;
        private readonly List<Action> actions = new();
        private readonly List<Action> onSelectActions = new();
        private readonly List<Action> onDeselectActions = new();

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
            if (Events.Status == SimulationStatus.Paused || Events.Status == SimulationStatus.Stopped)
            {
                StartButtonText.text = "Start";
            }
            else
            {
                StartButtonText.text = "Stop";
            }
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
            Debug.Log("Open Trash 0 Lid");
        }

        private void RequestCollectTrash0()
        {
            Debug.Log("Request Collect Trash 0, code: " + ActionInputField.text);
        }

        private void RingTrash1Buzzer()
        {
            Debug.Log("Ring Trash 1 Buzzer");
        }

        private void RequestCollectTrash1()
        {
            Debug.Log("Request Collect Trash 1, code: " + ActionInputField.text);
        }

        private void OpenTrash2Lid()
        {
            Debug.Log("Open Trash 2 Lid");
        }

        private void DisplayTextOnTrash2()
        {
            Debug.Log("Display Text on Trash 2, text: " + ActionInputField.text);
        }

        private void RequestCollectTrash2()
        {
            Debug.Log("Request Collect Trash 2, code: " + ActionInputField.text);
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
