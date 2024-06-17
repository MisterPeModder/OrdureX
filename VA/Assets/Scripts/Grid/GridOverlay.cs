using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace OrdureX.Grid
{
    public class GridOverlay : MonoBehaviour
    {
        [SerializeField]
        private GridManager m_Manager;

        [SerializeField]
        private Connectable m_Origin;

        [SerializeField]
        private GameObject m_Overlay;

        [SerializeField]
        private Vector3 m_TileScale = new Vector3(0.305f * 0.9021875f, 0.305f * 0.9021875f, 0.305f * 0.9021875f);

        public float TileSpacing
        {
            get => m_Manager.TileSpacing;
            set => m_Manager.TileSpacing = value;
        }

        [SerializeField]
        private List<Vector3> m_Vertices = new();

        [SerializeField]
        private List<Vector2> m_UVs = new();
        [SerializeField]
        private List<int> m_Triangles = new();

        public void Initialize(GridManager manager, Connectable origin)
        {
            m_Manager = manager;
            m_Vertices.Clear();
            m_Origin = origin;

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

            m_Overlay = new GameObject("Grid Overlay Mesh", typeof(MeshFilter), typeof(MeshRenderer));
            var mesh = m_Overlay.GetComponent<MeshFilter>().mesh;
            var renderer = m_Overlay.GetComponent<MeshRenderer>();

            renderer.material = manager.GridOverlayMaterial;
            renderer.material.color = new Color(origin.HighlightColor.r, origin.HighlightColor.g, origin.HighlightColor.b, 0.5f);

            mesh.vertices = m_Vertices.ToArray();
            mesh.uv = m_UVs.ToArray();
            mesh.triangles = m_Triangles.ToArray();

            m_Overlay.transform.SetParent(origin.transform, false);
            m_Overlay.transform.localPosition += new Vector3(0, 0.005f, 0);
            m_Overlay.transform.localEulerAngles = new Vector3(0, 180, 0);

            m_Overlay.transform.localScale = m_TileScale;
        }

        private GridTile CreateTile(Connectable original, Vector3Int pos, float angle)
        {
            Vector3 scaledPos = pos;
            scaledPos.x *= TileSpacing;
            scaledPos.z *= TileSpacing;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            GameObject tileObject = new();
            tileObject.SetActive(false);
            tileObject.transform.SetParent(transform);
            tileObject.transform.rotation = rotation;
            tileObject.transform.localPosition = scaledPos;
            tileObject.name = "Overlay Marker (" + original.name + ")";
            GridTile tile = tileObject.AddComponent<GridTile>();
            tile.GridPos = pos;
            tile.Angle = angle;

            m_Triangles.Add(m_Vertices.Count);
            m_Triangles.Add(m_Vertices.Count + 1);
            m_Triangles.Add(m_Vertices.Count + 2);
            m_Triangles.Add(m_Vertices.Count);
            m_Triangles.Add(m_Vertices.Count + 2);
            m_Triangles.Add(m_Vertices.Count + 3);

            m_Vertices.Add(pos + new Vector3(-0.5f, 0, -0.5f));
            m_Vertices.Add(pos + new Vector3(-0.5f, 0, 0.5f));
            m_Vertices.Add(pos + new Vector3(0.5f, 0, 0.5f));
            m_Vertices.Add(pos + new Vector3(0.5f, 0, -0.5f));

            m_UVs.Add(new Vector2(0, 0));
            m_UVs.Add(new Vector2(0, 1));
            m_UVs.Add(new Vector2(1, 1));
            m_UVs.Add(new Vector2(1, 0));
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
            if (m_Overlay != null)
            {
                Destroy(m_Overlay);
                m_Overlay = null;
            }
        }
    }
}
