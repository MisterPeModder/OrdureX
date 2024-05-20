using System.Collections;
using UnityEngine;

namespace OrdureX
{
    /// <summary>
    /// Limits the frame rate to the specified value when in play mode.
    /// </summary>
    public class FrameRateLimiter : MonoBehaviour
    {

        public int FrameRate = 60;

        public void Start()
        {
            StartCoroutine(ChangeFramerate());
        }

        private IEnumerator ChangeFramerate()
        {
            yield return new WaitForSeconds(1);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = FrameRate;
        }
    }
}
