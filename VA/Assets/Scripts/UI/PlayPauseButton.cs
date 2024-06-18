using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace OrdureX.UI
{
    [RequireComponent(typeof(Toggle))]
    public class PlayPauseButton : MonoBehaviour
    {
        [SerializeField]
        private Sprite m_OffSprite;

        [SerializeField]
        private Sprite m_OnSprite;

        [SerializeField]
        private Sprite m_LoaderSprite;

        [SerializeField]
        private Image m_TargetGraphic;

        private Toggle m_Button;

        private bool m_IsLoading = false;

        private void Awake()
        {
            m_Button = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            UpdateGraphic(m_Button.isOn);
            m_Button.onValueChanged.AddListener(UpdateGraphic);
        }

        private void OnDisable()
        {
            m_Button.onValueChanged.RemoveListener(UpdateGraphic);
        }

        private void Update()
        {
            if (!m_IsLoading)
            {
                return;
            }

            m_TargetGraphic.transform.Rotate(0, 0, 360 * Time.deltaTime);
        }


        public void UpdateGraphic(bool value)
        {
            m_IsLoading = !m_Button.IsInteractable();
            if (m_IsLoading)
            {
                m_TargetGraphic.sprite = m_LoaderSprite;
            }
            else
            {
                m_TargetGraphic.transform.rotation = Quaternion.identity;
                m_TargetGraphic.sprite = value ? m_OnSprite : m_OffSprite;
            }
        }

    }
}
