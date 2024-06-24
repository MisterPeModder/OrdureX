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

        private SimulationStateManager m_SimulationStateManager;

        private bool m_IsLoading = false;

        private void Awake()
        {
            m_Button = GetComponent<Toggle>();
            m_SimulationStateManager = FindObjectOfType<SimulationStateManager>();
        }

        private void OnEnable()
        {
            UpdateGraphic(m_Button.isOn);
            m_Button.onValueChanged.AddListener(UpdateGraphic);
            m_SimulationStateManager.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            m_Button.onValueChanged.RemoveListener(UpdateGraphic);
            m_SimulationStateManager.OnStatusChanged -= OnStatusChanged;
        }

        private void Update()
        {
            if (!m_IsLoading)
            {
                return;
            }

            m_TargetGraphic.transform.Rotate(0, 0, 360 * Time.deltaTime);
        }

        private void OnStatusChanged(SimulationStatus prevStatus, SimulationStatus newStatus)
        {
            UpdateGraphic(m_Button.isOn);
        }

        public void UpdateGraphic(bool value)
        {
            var status = m_SimulationStateManager.Status;
            if (status == SimulationStatus.Connecting)
            {
                m_TargetGraphic.sprite = m_LoaderSprite;
            }
            else
            {
                m_TargetGraphic.transform.rotation = Quaternion.identity;
                m_TargetGraphic.sprite = value ? m_OnSprite : m_OffSprite;
            }
            m_Button.interactable = status == SimulationStatus.Paused || status == SimulationStatus.Running;
            m_TargetGraphic.color = m_Button.interactable ? Color.white : Color.gray;
        }

    }
}
