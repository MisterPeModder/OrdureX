using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OrdureX.UI
{
    [RequireComponent(typeof(TrashHologramDisplay))]
    public class Trash1EventListener : MonoBehaviour
    {
        [SerializeField]
        private OrdureXEvents m_Events;
        private TrashHologramDisplay m_Display;

        [Header("Status")]
        [SerializeField]
        private bool m_IsCollectRequested = false;
        [SerializeField]
        private bool m_HasInvalidCode = false;
        [SerializeField]
        private bool m_IsBurning = false;

        private void Awake()
        {
            if (m_Events == null)
            {
                m_Events = FindObjectOfType<OrdureXEvents>();
            }
            m_Display = GetComponent<TrashHologramDisplay>();
        }

        private void OnEnable()
        {
            m_Events.OnTrash1CollectRequested += OnCollectRequested;
            m_Events.OnTrash1InvalidCode += OnInvalidCode;
            m_Events.OnTrash1BurningChanged += OnBurningChanged;
        }

        private void OnDisable()
        {
            m_Events.OnTrash1CollectRequested -= OnCollectRequested;
            m_Events.OnTrash1InvalidCode -= OnInvalidCode;
            m_Events.OnTrash1BurningChanged -= OnBurningChanged;
        }


        // Start is called before the first frame update
        private void Start()
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            List<string> status = new();

            if (m_IsCollectRequested)
            {
                status.Add("<color=#00ffffff>collect requested</color>");
            }
            if (m_HasInvalidCode)
            {
                status.Add("<color=#a52a2aff>invalid code</color>");
            }
            if (m_IsBurning)
            {
                status.Add("<color=#ffa500ff>burning</color>");
            }

            m_Display.Title = "Trash #1";
            m_Display.Status = status.Any() ? string.Join("\n", status) : "idle";
            m_Display.FireActive = m_IsBurning;
        }

        private void OnCollectRequested(bool value)
        {
            m_IsCollectRequested = value;
            UpdateStatus();
        }

        private void OnInvalidCode(Guid uuid)
        {
            Debug.Log("Invalid code for trash 1: " + uuid.ToString());
            m_HasInvalidCode = true;
            UpdateStatus();
        }

        private void OnBurningChanged(bool isBurning)
        {
            m_IsBurning = isBurning;
            UpdateStatus();
        }
    }
}
