using OrdureX.AR;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace OrdureX
{
    [RequireComponent(typeof(Collider))]
    public class TileConnector : MonoBehaviour
    {
        public SurfaceTile OwnTile;

        public SurfaceTile ConnectedTile
        {
            get
            {
                if (m_ConnectedTo == null)
                {
                    return null;
                }
                return m_ConnectedTo.OwnTile;
            }
        }

        private TileConnector m_ConnectedTo;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<TileConnector>(out var otherConnector))
            {
                m_ConnectedTo = otherConnector;
                Debug.Log($"Tile Connection: {transform.position} -> {other.transform.position}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<TileConnector>(out var otherConnector) && otherConnector == m_ConnectedTo)
            {
                m_ConnectedTo = null;
                Debug.Log($"Tile Disconnection: {transform.position} -> {other.transform.position}");
            }
        }
    }
}
