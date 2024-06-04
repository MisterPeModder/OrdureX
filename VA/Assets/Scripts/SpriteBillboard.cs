using UnityEngine;

namespace OrdureX
{
    public class SpriteBillboard : MonoBehaviour
    {
        private void Update()
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
