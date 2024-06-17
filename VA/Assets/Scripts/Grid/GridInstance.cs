using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace OrdureX.Grid
{
    public class GridInstance : MonoBehaviour
    {
        [SerializeField]
        private GridManager m_Manager;

        [SerializeField]
        private NavMeshSurface m_NavMeshSurface;

        public float TileSpacing
        {
            get => m_Manager.TileSpacing;
            set => m_Manager.TileSpacing = value;
        }

        public void Initialize(GridManager manager, Connectable origin)
        {
            m_Manager = manager;

            List<Connectable> toVisit = new() { origin };
            Dictionary<Connectable, GridTile> tiles = new() { { origin, CreateTile(origin, Vector3Int.zero, 0) } };

            while (toVisit.Count > 0)
            {
                Connectable current = toVisit[^1];
                GridTile currentTile = tiles[current];
                toVisit.RemoveAt(toVisit.Count - 1);

                SpawnNeighborTile(current, currentTile, Side.Top, toVisit, tiles);
                SpawnNeighborTile(current, currentTile, Side.Bottom, toVisit, tiles);
                SpawnNeighborTile(current, currentTile, Side.Right, toVisit, tiles);
                SpawnNeighborTile(current, currentTile, Side.Left, toVisit, tiles);
            }

            Debug.Log("Grid fully created!");
        }

        private void Start()
        {
            Debug.Log("Start() called");

            if (m_NavMeshSurface != null || TryGetComponent(out m_NavMeshSurface))
            {
                Debug.Log("Found NavMeshSurface component");
                m_NavMeshSurface.BuildNavMesh();
                Debug.Log("Built NavMesh");
            }
        }

        private GridTile CreateTile(Connectable original, Vector3Int pos, float angle)
        {
            Vector3 scaledPos = pos;
            scaledPos.x *= TileSpacing;
            scaledPos.z *= TileSpacing;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            GameObject tileObject = Instantiate(original.PathPrefab, Vector3.zero, rotation, transform);
            tileObject.transform.localPosition = scaledPos;
            tileObject.name = "GridTile (" + original.name + ")";
            GridTile tile = tileObject.AddComponent<GridTile>();
            tile.GridPos = pos;
            tile.Angle = angle;
            return tile;
        }

        private void SpawnNeighborTile(Connectable current, GridTile currentTile, Side toSide, IList<Connectable> toVisit, IDictionary<Connectable, GridTile> tiles)
        {
            Connector currentConnector = current.GetConnector(toSide);
            Connectable neighbor = currentConnector.Touching;
            if (neighbor != null && !tiles.ContainsKey(neighbor))
            {
                Connector neighborConnector = currentConnector.TouchingConnector;
                toVisit.Add(neighbor);

                float angle = toSide.GetRotation() - neighborConnector.Side.GetOpposite().GetRotation();
                angle = Mathf.Round((angle + currentTile.Angle) / 90) * 90;

                Vector3 direction = new(toSide.GetDirection().x, 0, toSide.GetDirection().y);
                direction = Quaternion.Euler(0, currentTile.Angle, 0) * direction;

                tiles[neighbor] = CreateTile(neighbor, currentTile.GridPos + Vector3Int.RoundToInt(direction), angle);
            }
        }

        private void OnDestroy()
        {
            m_Manager.OnGridDestroyed(this);
        }
    }
}
