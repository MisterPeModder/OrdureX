using UnityEngine;

namespace OrdureX.Grid
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Connectable))]
    public class ConnectableExampleOverlay : MonoBehaviour
    {
        private Renderer m_Renderer;
        private Connectable m_Connectable;

        void Start()
        {
            m_Renderer = GetComponent<Renderer>();
            m_Connectable = GetComponent<Connectable>();
        }

        void Update()
        {
            m_Renderer.material.color = m_Connectable.HighlightColor;
        }
    }

}
