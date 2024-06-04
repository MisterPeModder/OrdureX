using TMPro;
using UnityEngine;

namespace OrdureX.AR
{
    public class SurfaceTile : MonoBehaviour
    {
        [SerializeField] private GameObject m_Overlay;
        [SerializeField] private TMP_Text m_OverlayTitle;

        public void Activate(string title)
        {
            Debug.Log($"Activating tile with title: {title}");
            m_OverlayTitle.text = title;
        }

    }
}
