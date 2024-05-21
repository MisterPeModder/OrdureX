using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrdureX
{
    public class StartStopButton : MonoBehaviour
    {
        public OrdureXEvents Events;

        public TMP_Text ButtonText;
        public Button Button;


        // Update is called once per frame
        void Update()
        {
            Button.interactable = Events.Connected;
            if (Events.Status == SimulationStatus.Paused || Events.Status == SimulationStatus.Stopped)
            {
                ButtonText.text = "Start";
            }
            else
            {
                ButtonText.text = "Stop";
            }
        }
    }
}
