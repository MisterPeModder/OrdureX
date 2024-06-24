using TMPro;
using UnityEngine;
using OrdureX.Grid;
using Unity.XR.CoreUtils;

namespace OrdureX.AR
{
    [RequireComponent(typeof(Connectable))]
    public class SurfaceTile : MonoBehaviour
    {
        [SerializeField] private Renderer m_Overlay;
        [SerializeField] private TMP_Text m_OverlayTitle;

        private Connectable m_Connectable;
        private SettingsManager m_SettingsManager;

        private void Awake()
        {
            m_SettingsManager = FindObjectOfType<SettingsManager>();
        }

        public void Activate(SurfaceTileManager manager, string title, GameObject pathPrefab)
        {
            Debug.Log($"Activating tile with title: {title}");
            m_OverlayTitle.text = title;
            m_Connectable = GetComponent<Connectable>();
            m_Connectable.PathPrefab = pathPrefab;
        }

        private void Update()
        {
            m_Overlay.gameObject.SetActive(m_SettingsManager.ShowTileDebugOverlay);

            if (m_Connectable == null || !m_SettingsManager.ShowTileDebugOverlay)
            {
                return;
            }

            // Snap overlay title to 90 degree increments
            m_OverlayTitle.transform.localRotation = Quaternion.Euler(90, GetHorizontalAngleToCamera(), 0);
            m_Overlay.material.color = m_Connectable.HighlightColor;
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
