using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace OrdureX.AR
{
    [RequireComponent(typeof(ARSession))]
    public class ARAvailibityChecker : MonoBehaviour
    {
        ARSession m_Session;

        void Awake()
        {
            m_Session = GetComponent<ARSession>();
        }

        IEnumerator Start()
        {
            if ((ARSession.state == ARSessionState.None) ||
                (ARSession.state == ARSessionState.CheckingAvailability))
            {
                Debug.Log("Checking AR availability");
                yield return ARSession.CheckAvailability();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                Debug.Log("AR not supported on this device");
            }
            else
            {
                Debug.Log("AR supported on this device: " + ARSession.state);
                // Start the AR session
                m_Session.enabled = true;
            }
        }
    }
}
