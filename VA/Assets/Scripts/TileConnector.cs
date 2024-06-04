using UnityEngine;

namespace OrdureX
{
    [RequireComponent(typeof(Collider))]
    public class TileConnector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Collision started: {transform.position} -> {other.transform.position}");
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"Collision ended: {transform.position} -> {other.transform.position}");
        }
    }
}
