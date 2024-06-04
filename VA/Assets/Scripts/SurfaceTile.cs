using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OrdureX.AR
{
    public class SurfaceTile : MonoBehaviour
    {
        [SerializeField] private bool m_ShowOverlay = true;
        [SerializeField] private GameObject m_Overlay;
        [SerializeField] private TMP_Text m_OverlayTitle;

        [Header("Connection Handling")]
        [SerializeField] private List<TileConnector> m_Connectors = new();
        [SerializeField] private GameObject m_ConnectionLinePrefab;
        [SerializeField] private List<LineRenderer> m_ConnectionLines = new();

        private SurfaceTileManager m_Manager;

        public void Activate(SurfaceTileManager manager, string title)
        {
            Debug.Log($"Activating tile with title: {title}");
            m_Manager = manager;
            m_OverlayTitle.text = title;

            for (int i = 0; i < m_Connectors.Count; i++)
            {
                var line = Instantiate(m_ConnectionLinePrefab, transform).GetComponent<LineRenderer>();
                m_ConnectionLines.Add(line);
            }
        }

        private void Update()
        {
            if (!m_ShowOverlay)
            {
                // When overlay is hidden, disable all connection lines
                foreach (var line in m_ConnectionLines)
                {
                    line.enabled = false;
                }
                return;
            }

            // Snap overlay title to 90 degree increments
            m_OverlayTitle.transform.localRotation = Quaternion.Euler(90, GetHorizontalAngleToCamera(), 0);
            UpdateConnectionLines();
        }

        private void UpdateConnectionLines()
        {
            for (int i = 0; i < m_ConnectionLines.Count; i++)
            {
                var line = m_ConnectionLines[i];
                var connector = m_Connectors[i];

                line.enabled = connector.ConnectedTile != null;
                if (!line.enabled)
                {
                    continue;
                }

                line.positionCount = 2;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, connector.ConnectedTile.transform.position);
            }
        }

        private float GetHorizontalAngleToCamera()
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;
            directionToCamera.y = 0; // Project onto horizontal plane

            Vector3 forwardOnPlane = transform.forward;
            forwardOnPlane.y = 0; // Project onto horizontal plane

            float angle = Vector3.SignedAngle(forwardOnPlane, directionToCamera, Vector3.up);
            angle = Mathf.Round((angle + 180) / 90) * 90;
            return angle;
        }

    }
}
