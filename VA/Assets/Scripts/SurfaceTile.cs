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

        [Header("Neighbor Detection")]
        [SerializeField] private int m_MaxNeighbors = 4;
        [SerializeField] private GameObject m_NeighborLinePrefab;
        [SerializeField] private List<LineRenderer> m_NeighborLines = new();
        [SerializeField] private float m_DistanceThreshold = 0.5f;

        private SurfaceTileManager m_Manager;

        public void Activate(SurfaceTileManager manager, string title)
        {
            Debug.Log($"Activating tile with title: {title}");
            m_Manager = manager;
            m_OverlayTitle.text = title;

            for (int i = 0; i < m_MaxNeighbors; i++)
            {
                var line = Instantiate(m_NeighborLinePrefab, transform).GetComponent<LineRenderer>();
                m_NeighborLines.Add(line);
            }
        }

        private void Update()
        {
            if (!m_ShowOverlay)
            {
                // When overlay is hidden, disable all neighbor lines
                foreach (var line in m_NeighborLines)
                {
                    line.enabled = false;
                }
                return;
            }

            // Snap overlay title to 90 degree increments
            m_OverlayTitle.transform.localRotation = Quaternion.Euler(90, GetHorizontalAngleToCamera(), 0);

            var neighbors = FindNeighbors();
            int i = 0;

            for (; i < neighbors.Count; i++)
            {
                var line = m_NeighborLines[i];
                var neighbor = neighbors[i];
                line.enabled = false;
                line.positionCount = 2;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, neighbor.transform.position);
            }

            for (; i < m_NeighborLines.Count; i++)
            {
                m_NeighborLines[i].enabled = false;
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

        private List<SurfaceTile> FindNeighbors()
        {
            List<Tuple<SurfaceTile, float>> tilesByDistance = new();

            foreach (var tile in m_Manager.Tiles)
            {
                if (tile == this)
                {
                    continue;
                }

                tilesByDistance.Add(new(tile, Vector3.Distance(transform.position, tile.transform.position)));
            }
            List<SurfaceTile> neighbors = new(m_MaxNeighbors);

            tilesByDistance.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            foreach (var (tile, distance) in tilesByDistance)
            {
                if (neighbors.Contains(tile) || distance > m_DistanceThreshold)
                {
                    continue;
                }

                neighbors.Add(tile);
                if (neighbors.Count >= m_MaxNeighbors)
                {
                    break;
                }
            }

            return neighbors;
        }

    }
}
