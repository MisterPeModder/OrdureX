using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace OrdureX.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField]
        private float m_GridSearchUpdateInterval = 1f;

        [SerializeField]
        private GameObject m_GridTilePrefab;
        [SerializeField]
        private GameObject m_GridInstancePrefab;

        public GameObject GridTilePrefab
        {
            get => m_GridTilePrefab;
            set => m_GridTilePrefab = value;
        }

        [SerializeField]
        private GridInstance m_GridInstance;

        [SerializeField]
        private float m_TileSpacing = 1f;

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
        }

        public void OnGridDestroyed(GridInstance grid)
        {
            if (ReferenceEquals(m_GridInstance, grid))
            {
                m_GridInstance = null;
            }
        }
    }
}
