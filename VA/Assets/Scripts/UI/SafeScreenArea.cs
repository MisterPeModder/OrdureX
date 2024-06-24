using UnityEngine;

namespace OrdureX.UI
{
    /// <summary>
    /// Scales the rect transform to fit the safe area of the screen.
    /// Useful for ignoring notches, rounded corners, camera holes, etc.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeScreenArea : MonoBehaviour
    {
        [SerializeField]
        float m_MarginTop = 0.0f;
        [SerializeField]
        float m_MarginBottom = 0.0f;
        [SerializeField]
        float m_MarginRight = 0.0f;
        [SerializeField]
        float m_MarginLeft = 0.0f;

        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            var safeArea = Screen.safeArea;
            var minAnchor = safeArea.position;
            var maxAnchor = safeArea.position + safeArea.size;

            minAnchor.x = (minAnchor.x + m_MarginRight) / Screen.width;
            minAnchor.y = (minAnchor.y + m_MarginBottom) / Screen.height;
            maxAnchor.x = (maxAnchor.x - m_MarginLeft) / Screen.width;
            maxAnchor.y = (maxAnchor.y - m_MarginTop) / Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
}
