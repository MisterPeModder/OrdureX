using TMPro;
using UnityEngine;
using OrdureX.Grid;
using System.Xml.Serialization;

namespace OrdureX.AR
{
    [RequireComponent(typeof(Connectable))]
    public class SurfaceTile : MonoBehaviour
    {
        [SerializeField] private bool m_ShowOverlay = true;
        [SerializeField] private Renderer m_Overlay;
        [SerializeField] private TMP_Text m_OverlayTitle;

        private Connectable m_Connectable;

        public void FirstHoverEntered()
        {
            Debug.Log("First hover entered");
        }

        public void LastHoverExited()
        {
            Debug.Log("Last hover exited");
        }

        public void HoverEntered()
        {
            Debug.Log("Hover entered");
        }

        public void HoverExited()
        {
            Debug.Log("Hover exited");
        }

        public void FirstSelectEntered()
        {
            Debug.Log("First select entered");
        }

        public void LastSelectExited()
        {
            Debug.Log("Last select exited");
        }

        public void SelectEntered()
        {
            Debug.Log("Select entered");
        }

        public void SelectExited()
        {
            Debug.Log("Select exited");
        }

        public void FirstFocusEntered()
        {
            Debug.Log("First focus entered");
        }

        public void LastFocusExited()
        {
            Debug.Log("Last focus exited");
        }

        public void FocusEntered()
        {
            Debug.Log("Focus entered");
        }

        public void FocusExited()
        {
            Debug.Log("Focus exited");
        }

        public void Activated()
        {
            Debug.Log("Activated");
        }

        public void Deactivated()
        {
            Debug.Log("Deactivated");
        }

        private void Start()
        {
            m_Connectable = GetComponent<Connectable>();
        }

        public void Activate(SurfaceTileManager manager, string title)
        {
            Debug.Log($"Activating tile with title: {title}");
            m_OverlayTitle.text = title;
        }

        private void Update()
        {
            if (!m_ShowOverlay)
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
