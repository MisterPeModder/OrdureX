using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OrdureX
{
    [RequireComponent(typeof(Renderer))]
    public class RenderOnlyInEditor : MonoBehaviour
    {
        private Renderer m_Renderer;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            m_Renderer.enabled = false;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayStateChanged(PlayModeStateChange state)
        {
            m_Renderer.enabled = state != PlayModeStateChange.EnteredPlayMode;
        }
#endif
    }
}
