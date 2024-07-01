using System;
using TMPro;
using UnityEngine;

namespace OrdureX.UI
{
    public class TrashHologramDisplay : MonoBehaviour
    {
        [Header("Objects")]
        [SerializeField]
        private Camera m_CameraToTrack;
        [SerializeField]
        private Canvas m_Canvas;
        [SerializeField]
        private Transform m_Display;
        [SerializeField]
        private TMP_Text m_TitleText;
        [SerializeField]
        private TMP_Text m_StatusText;


        [Header("Display Settings")]
        [SerializeField]
        private string m_Title = "Trash #?";
        public string Title
        {
            get => m_TitleText == null ? m_Title : m_TitleText.text;
            set
            {
                if (m_TitleText != null)
                {
                    m_TitleText.text = value;
                }
                m_Title = value;
            }
        }
        [SerializeField]
        private string m_Status = "closed";
        public string Status
        {
            get => m_StatusText == null ? m_Status : m_StatusText.text;
            set
            {
                if (m_StatusText != null)
                {
                    m_StatusText.text = value;
                }
                m_Status = value;
            }
        }
        [SerializeField]
        private Vector3 m_BaseScale;
        [SerializeField]
        private float m_DistanceToScaleFactor = 0.3f;

        private void Awake()
        {
            if (m_CameraToTrack == null)
            {
                m_CameraToTrack = Camera.main;
            }
            m_Canvas.worldCamera = m_CameraToTrack;
            if (m_BaseScale == null && m_Display != null)
            {
                m_BaseScale = m_Display.transform.localScale;
            }
            if (m_TitleText != null)
            {
                m_TitleText.text = m_Title;
            }
            if (m_StatusText != null)
            {
                m_StatusText.text = m_Status;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_CameraToTrack == null)
            {
                return;
            }

            m_Display.transform.rotation = Quaternion.LookRotation(m_Display.transform.position - m_CameraToTrack.transform.position);

            float distanceFromCamera = Vector3.Distance(m_Display.transform.position, m_CameraToTrack.transform.position);

            m_Display.transform.localScale = m_BaseScale * (distanceFromCamera / m_DistanceToScaleFactor);
        }
    }
}
