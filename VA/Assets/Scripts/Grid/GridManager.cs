using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OrdureX.UI;

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
        private List<GridOverlayMesh> m_GridOverlays = new();

        [SerializeField]
        private Material[] m_GridOverlayMaterials;

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
        private List<ScaledProjection> m_TrashCanProjectionPrefabs = new();
        private int m_TrashCanPrefabIndex = -1;
        [SerializeField]
        private GameObject m_TruckInstance;
        [SerializeField]
        private ScaledProjection m_TruckProjectionInstance;
        [SerializeField]
        private List<GameObject> m_TrashCanInstances = new();
        [SerializeField]
        private List<ScaledProjection> m_TrashCanProjectionInstances = new();
        [SerializeField]
        private float m_MinimumTruckToTrashCanDistance = 3.0f;
        [SerializeField]
        private int m_TrashCansToSpawn = 3;

        private SimulationStateManager m_SimulationStateManager;
        private OrdureXEvents m_Events;

        public Material[] GridOverlayMaterials
        {
            get => m_GridOverlayMaterials;
            set => m_GridOverlayMaterials = value;
        }

        public float TileSpacing
        {
            get => m_TileSpacing;
            set => m_TileSpacing = value;
        }


        private void Awake()
        {
            m_SimulationStateManager = FindObjectOfType<SimulationStateManager>();
            m_Events = FindObjectOfType<OrdureXEvents>();
        }


        private void Start()
        {
            m_GridInstance = null;
            DestroyOverlays();
            if (m_GridInstancePrefab == null)
            {
                m_GridInstancePrefab = new GameObject("GridInstance");
            }
            StartCoroutine(SearchForGrids());
        }

        private void OnEnable()
        {
            m_SimulationStateManager.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            m_SimulationStateManager.OnStatusChanged -= OnStatusChanged;
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

            List<ISet<Connectable>> connectedGrids = new();

            foreach (Connectable connectable in toConnect)
            {
                connectable.AllTouching = connectable.NextAllTouching;
                connectable.NextAllTouching = null;
                if (!connectedGrids.Exists(grid => ReferenceEquals(grid, connectable.AllTouching)))
                {
                    connectedGrids.Add(connectable.AllTouching);
                }
            }

            DestroyOverlays();
            foreach (ISet<Connectable> connectedGrid in connectedGrids)
            {
                AddOverlay(connectedGrid.First());
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
            if (m_GridInstance != null || origin.AllTouching == null || origin.AllTouching.Count == 0)
            {
                return;
            }

            m_Events.OnStartOrStop();

            DestroyOverlays();
            origin = origin.AllTouching.First();

            m_GridInstance = Instantiate(m_GridInstancePrefab, transform).AddComponent<GridInstance>();
            m_GridInstance.transform.SetParent(transform);
            m_GridInstance.transform.localPosition = Vector3.zero;
            m_GridInstance.Initialize(this, origin);

            var spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point");
            RespawnTruck(spawnPoints);
            RespawnTrashCans(spawnPoints);
            SpawnTruckProjection(origin);
            SpawnTrashCanProjections(origin);
        }

        private void AddOverlay(Connectable origin)
        {
            var gridOverlay = new GameObject("Candidate Grid Overlay").AddComponent<GridOverlayMesh>();
            gridOverlay.Initialize(this, origin);
            m_GridOverlays.Add(gridOverlay);
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

        private void RespawnTrashCans(GameObject[] spawnPoints)
        {
            if (m_TrashCanPrefab == null)
            {
                Debug.LogWarning("No trash can prefabs set, cannot spawn");
                return;
            }

            foreach (var trashCan in m_TrashCanInstances)
            {
                Destroy(trashCan);
            }
            m_TrashCanInstances.Clear();

            var remainingSpawnPoints = new List<GameObject>(spawnPoints);
            var trashCansToSpawn = m_TrashCansToSpawn;

            remainingSpawnPoints.RemoveAll(spawnPoint => Vector3.Distance(spawnPoint.transform.position, m_TruckInstance.transform.position) < m_MinimumTruckToTrashCanDistance);

            if (remainingSpawnPoints.Count == 0)
            {
                Debug.LogWarning("No spawn points found, cannot spawn trash can");
                return;
            }

            while (trashCansToSpawn > 0 && remainingSpawnPoints.Count > 0)
            {
                var spawnPoint = remainingSpawnPoints[Random.Range(0, remainingSpawnPoints.Count)];
                remainingSpawnPoints.Remove(spawnPoint);

                var trashCanInstance = Instantiate(m_TrashCanPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
                trashCanInstance.transform.SetParent(transform);
                m_TrashCanInstances.Add(trashCanInstance);
                trashCansToSpawn--;
            }

            m_TruckInstance.GetComponent<GarbageTruckAI>().Targets = m_TrashCanInstances.Select(trashCan => trashCan.transform).ToList();
        }

        private void SpawnTruckProjection(Connectable origin)
        {
            if (m_TruckProjectionPrefab == null)
            {
                Debug.LogWarning("Mini truck prefab not set, cannot spawn");
                return;
            }

            DestroyTruckProjection();

            m_TruckProjectionInstance = Instantiate(m_TruckProjectionPrefab, origin.transform.position, origin.transform.rotation);
            m_TruckProjectionInstance.transform.SetParent(origin.transform);
            m_TruckProjectionInstance.Initialize(transform, origin.transform, m_TruckInstance.transform);
        }

        private void SpawnTrashCanProjections(Connectable origin)
        {
            if (m_TrashCanProjectionPrefabs.Count == 0)
            {
                Debug.LogWarning("Trash can projection not set, cannot spawn");
                return;
            }

            DestroyTrashCanProjections();

            foreach (var trashCan in m_TrashCanInstances)
            {
                m_TrashCanPrefabIndex = (m_TrashCanPrefabIndex + 1) % m_TrashCanProjectionPrefabs.Count;

                var trashCanProjectionInstance = Instantiate(m_TrashCanProjectionPrefabs[m_TrashCanPrefabIndex], origin.transform.position, origin.transform.rotation);
                trashCanProjectionInstance.transform.SetParent(origin.transform);

                switch (m_TrashCanPrefabIndex)
                {
                    case 0:
                        trashCanProjectionInstance.gameObject.AddComponent<Trash0EventListener>();
                        break;
                    case 1:
                        trashCanProjectionInstance.gameObject.AddComponent<Trash1EventListener>();
                        break;
                    case 2:
                        trashCanProjectionInstance.gameObject.AddComponent<Trash2EventListener>();
                        break;
                }

                trashCanProjectionInstance.Initialize(transform, origin.transform, trashCan.transform);
                m_TrashCanProjectionInstances.Add(trashCanProjectionInstance);
            }
        }

        private void DestroyOverlays()
        {
            foreach (GridOverlayMesh overlay in m_GridOverlays)
            {
                if (overlay != null)
                {
                    Destroy(overlay.gameObject);
                }
            }
            m_GridOverlays.Clear();
        }

        private void DestroyGrid()
        {
            if (m_GridInstance != null)
            {
                Destroy(m_GridInstance.gameObject);
                m_GridInstance = null;
            }
        }

        private void DestroyTruckProjection()
        {
            if (m_TruckProjectionInstance != null)
            {
                Destroy(m_TruckProjectionInstance.gameObject);
                m_TruckProjectionInstance = null;
            }
        }

        private void DestroyTrashCanProjections()
        {
            foreach (var trashCanProjection in m_TrashCanProjectionInstances)
            {
                if (trashCanProjection != null)
                {
                    Destroy(trashCanProjection.gameObject);
                }
            }
            m_TrashCanProjectionInstances.Clear();
        }

        private void OnDestroy()
        {
            DestroyTruckProjection();
            DestroyTrashCanProjections();
            DestroyOverlays();
        }

        private void OnStatusChanged(SimulationStatus prevStatus, SimulationStatus newStatus)
        {
            if (newStatus == SimulationStatus.Stopped)
            {
                DestroyGrid();
                DestroyTruckProjection();
                DestroyTrashCanProjections();
                DestroyOverlays();
            }
        }
    }
}
