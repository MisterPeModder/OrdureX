using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrdureX.UI
{
    [RequireComponent(typeof(TrashHologramDisplay))]
    public class Trash2EventListener : MonoBehaviour
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
        private bool m_IsLidOpen = false;

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
            m_Events.OnTrash2CollectRequested += OnCollectRequested;
            m_Events.OnTrash2InvalidCode += OnInvalidCode;
            m_Events.OnTrash2LidChanged += OnLidChanged;
        }

        private void OnDisable()
        {
            m_Events.OnTrash2CollectRequested -= OnCollectRequested;
            m_Events.OnTrash2InvalidCode -= OnInvalidCode;
            m_Events.OnTrash2LidChanged -= OnLidChanged;
        }


        // Start is called before the first frame update
        private void Start()
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            List<string> status = new()
            {
                m_IsLidOpen ? "open" : "closed"
            };

            if (m_IsCollectRequested)
            {
                status.Add("collect requested");
            }
            if (m_HasInvalidCode)
            {
                status.Add("invalid code");
            }

            m_Display.Title = "Trash #2";
            m_Display.Status = string.Join(" | ", status);
        }

        private void OnCollectRequested()
        {
            m_IsCollectRequested = true;
            UpdateStatus();
        }

        private void OnInvalidCode(Guid uuid)
        {
            Debug.Log("Invalid code for trash 2: " + uuid.ToString());
            m_HasInvalidCode = true;
            UpdateStatus();
        }

        private void OnLidChanged(bool isLidOpen)
        {
            m_IsLidOpen = isLidOpen;
            UpdateStatus();
        }
    }
}
