using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

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
        float m_Margin = 0.0f;

        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            var safeArea = Screen.safeArea;
            var minAnchor = safeArea.position;
            var maxAnchor = safeArea.position + safeArea.size;

            minAnchor.x = (minAnchor.x + m_Margin) / Screen.width;
            minAnchor.y = (minAnchor.y + m_Margin) / Screen.height;
            maxAnchor.x = (maxAnchor.x - m_Margin) / Screen.width;
            maxAnchor.y = (maxAnchor.y - m_Margin) / Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
}
