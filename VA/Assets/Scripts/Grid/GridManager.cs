using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OrdureX.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField]
        private float m_GridSearchUpdateInterval = 1f;

        [SerializeField]
        private GameObject m_GridInstancePrefab;

        [SerializeField]
        private GridInstance m_GridInstance;

        [SerializeField]
        private float m_TileSpacing = 1f;

        [Header("Simulation Objects")]
        [SerializeField]
        private GameObject m_TruckPrefab;
        [SerializeField]
        private ScaledProjection m_TruckProjectionPrefab;
        [SerializeField]
        private GameObject m_TrashCanPrefab;
        [SerializeField]
        private ScaledProjection m_TrashCanProjectionPrefab;

        [SerializeField]
        private GameObject m_TruckInstance;
        [SerializeField]
        private ScaledProjection m_TruckProjectionInstance;
        [SerializeField]
        private GameObject m_TrashCanInstance;
        [SerializeField]
        private ScaledProjection m_TrashCanProjectionInstance;

        public float TileSpacing
        {
            get => m_TileSpacing;
            set => m_TileSpacing = value;
        }


        private void Start()
        {
            m_GridInstance = null;
            if (m_GridInstancePrefab == null)
            {
                m_GridInstancePrefab = new GameObject("GridInstance");
            }
            StartCoroutine(SearchForGrids());
        }

        private IEnumerator SearchForGrids()
        {
            while (true)
            {
                FindTouchingConnectables();
                yield return new WaitForSeconds(m_GridSearchUpdateInterval);
            }
        }

        /// <summary>
        /// Finds all touching connectables in the scene and groups them into connected grids.
        /// <para>NOTE: this opereration is expensive, do not call every frame!</para>
        /// </summary>
        private void FindTouchingConnectables()
        {
            if (m_GridInstance != null)
            {
                return;
            }
            HashSet<Connectable> toConnect = new(FindObjectsByType<Connectable>(FindObjectsSortMode.None));

            foreach (Connectable connectable in toConnect)
            {
                connectable.NextAllTouching = new HashSet<Connectable> { connectable };

                var newColor = Color.HSVToRGB(Random.value, 1f, 1f);
                newColor.a = 0.25f;
                connectable.HighlightColor = newColor;
            }

            foreach (Connectable local in toConnect)
            {
                NeighborConnect(local, local.TopConnector.Touching);
                NeighborConnect(local, local.BottomConnector.Touching);
                NeighborConnect(local, local.RightConnector.Touching);
                NeighborConnect(local, local.LeftConnector.Touching);
            }

            foreach (Connectable connectable in toConnect)
            {
                connectable.AllTouching = connectable.NextAllTouching;
                connectable.NextAllTouching = null;
            }
        }

        /// <summary>
        /// Merges two sets of connectables into one.
        /// </summary>
        private void NeighborConnect(Connectable local, Connectable neighbor)
        {
            if (neighbor == null)
            {
                return;
            }

            var localGrid = local.NextAllTouching;
            var neighborGrid = neighbor.NextAllTouching;

            if (ReferenceEquals(localGrid, neighborGrid))
            {
                // Already connected
                return;
            }

            if (localGrid.Count < neighborGrid.Count)
            {
                var neighborColor = neighbor.HighlightColor;
                foreach (var connectedToLocal in localGrid)
                {
                    connectedToLocal.HighlightColor = neighborColor;
                    connectedToLocal.NextAllTouching = neighborGrid;
                }
                neighborGrid.UnionWith(localGrid);
            }
            else
            {
                var localColor = local.HighlightColor;
                foreach (var connectedToNeighbor in neighborGrid)
                {
                    connectedToNeighbor.HighlightColor = localColor;
                    connectedToNeighbor.NextAllTouching = localGrid;
                }
                localGrid.UnionWith(neighborGrid);
            }
        }

        public void CreateGrid(Connectable origin)
        {
            if (m_GridInstance != null)
            {
                return;
            }

            m_GridInstance = Instantiate(m_GridInstancePrefab, transform).AddComponent<GridInstance>();
            m_GridInstance.transform.SetParent(transform);
            m_GridInstance.transform.localPosition = Vector3.zero;
            m_GridInstance.Initialize(this, origin);

            var spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point");
            RespawnTruck(spawnPoints);
            RespawnTrashCan(spawnPoints);
            SpawnTruckProjection(origin);
            SpawnTrashCanProjection(origin);
        }

        public void OnGridDestroyed(GridInstance grid)
        {
            if (ReferenceEquals(m_GridInstance, grid))
            {
                m_GridInstance = null;
            }
        }

        private void RespawnTruck(GameObject[] spawnPoints)
        {
            if (m_TruckPrefab == null)
            {
                Debug.LogWarning("Truck prefab not set, cannot spawn");
                return;
            }

            if (m_TruckInstance != null)
            {
                Destroy(m_TruckInstance);
                m_TruckInstance = null;
            }

            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points found, cannot spawn truck");
                return;
            }
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            m_TruckInstance = Instantiate(m_TruckPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            m_TruckInstance.transform.SetParent(transform);
        }

        private void RespawnTrashCan(GameObject[] spawnPoints)
        {
            if (m_TrashCanPrefab == null)
            {
                Debug.LogWarning("Trash prefab not set, cannot spawn");
                return;
            }

            if (m_TrashCanInstance != null)
            {
                Destroy(m_TrashCanInstance);
                m_TrashCanInstance = null;
            }

            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points found, cannot spawn truck");
                return;
            }

            var truckPos = m_TruckInstance.transform.position;
            var spawnPoint = spawnPoints
                .OrderByDescending(sp => Vector3.Distance(sp.transform.position, truckPos))
                .FirstOrDefault();

            if (spawnPoint != null)
            {
                m_TrashCanInstance = Instantiate(m_TrashCanPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
                m_TrashCanInstance.transform.SetParent(transform);

                m_TruckInstance.GetComponent<GarbageTruckAI>().Target = m_TrashCanInstance.transform;
            }
            else
            {
                Debug.LogWarning("No other spawn points found, cannot spawn trash can");
            }
        }

        private void SpawnTruckProjection(Connectable origin)
        {
            if (m_TruckProjectionPrefab == null)
            {
                Debug.LogWarning("Mini truck prefab not set, cannot spawn");
                return;
            }

            if (m_TruckProjectionInstance != null)
            {
                Destroy(m_TruckProjectionInstance.gameObject);
                m_TruckProjectionInstance = null;
            }

            m_TruckProjectionInstance = Instantiate(m_TruckProjectionPrefab, origin.transform.position, origin.transform.rotation);
            m_TruckProjectionInstance.transform.SetParent(origin.transform);
            m_TruckProjectionInstance.Initialize(transform, origin.transform, m_TruckInstance.transform);
        }

        private void SpawnTrashCanProjection(Connectable origin)
        {
            if (m_TrashCanProjectionPrefab == null)
            {
                Debug.LogWarning("Mini truck prefab not set, cannot spawn");
                return;
            }

            if (m_TrashCanProjectionInstance != null)
            {
                Destroy(m_TrashCanProjectionInstance.gameObject);
                m_TrashCanProjectionInstance = null;
            }

            m_TrashCanProjectionInstance = Instantiate(m_TrashCanProjectionPrefab, origin.transform.position, origin.transform.rotation);
            m_TrashCanProjectionInstance.transform.SetParent(origin.transform);
            m_TrashCanProjectionInstance.Initialize(transform, origin.transform, m_TrashCanInstance.transform);
        }

    }
}
