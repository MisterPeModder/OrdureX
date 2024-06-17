using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrdureX
{
    [ExecuteInEditMode]
    public class ScaledProjection : MonoBehaviour
    {
        [SerializeField]
        private Transform m_BigOrigin;

        [SerializeField]
        private Transform m_ToTrack;

        [SerializeField]
        private Transform m_SmallOrigin;

        [SerializeField]
        private float m_ScaleFactor = 1.0f;

        [SerializeField]
        [Tooltip("Rotation offset to apply to the big origin")]
        private Quaternion m_OriginRotationOffset = Quaternion.identity;

        public void Initialize(Transform bigOrigin, Transform smallOrigin, Transform toTrack)
        {
            m_BigOrigin = bigOrigin;
            m_SmallOrigin = smallOrigin;
            m_ToTrack = toTrack;
        }

        private void Update()
        {
            if (m_ToTrack == null || m_BigOrigin == null || m_SmallOrigin == null)
            {
                return; // Early exit if any of the references are missing
            }


            // Calculate the position of m_ToTrack relative to m_BigOrigin
            Vector3 relativePosition = m_BigOrigin.InverseTransformPoint(m_ToTrack.position);

            // rotate relative position by 180 around big origin
            relativePosition = m_OriginRotationOffset * relativePosition;

            Quaternion relativeRotation = Quaternion.Inverse(m_BigOrigin.rotation * m_OriginRotationOffset) * m_ToTrack.rotation;

            // Apply any known scale difference between the contexts
            // Assuming scaleFactor is a float representing this difference
            relativePosition *= m_ScaleFactor;

            // Set this GameObject's position and rotation relative to m_SmallOrigin
            transform.SetPositionAndRotation(m_SmallOrigin.TransformPoint(relativePosition), m_SmallOrigin.rotation * relativeRotation);
        }
    }
}
