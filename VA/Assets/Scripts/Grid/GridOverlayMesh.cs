using System.Collections.Generic;
using UnityEngine;

namespace OrdureX.Grid
{
    /// <summary>
    /// Dynamic mesh that cover the surface of a grid of touching Connectables.
    /// </summary>
    public class GridOverlayMesh : MonoBehaviour
    {
        [SerializeField]
        private GridManager m_Manager;

        [SerializeField]
        private Connectable m_Origin;

        [SerializeField]
        private Vector3 m_TileScale = new Vector3(0.305f * 0.9021875f, 0.305f * 0.9021875f, 0.305f * 0.9021875f);

        public float TileSpacing
        {
            get => m_Manager.TileSpacing;
            set => m_Manager.TileSpacing = value;
        }

        [SerializeField]
        private List<Vector3> m_Vertices = new();

        private readonly List<Vector3> m_Normals = new();
        private readonly List<Vector2> m_UVs = new();
        private readonly List<int> m_Triangles = new();

        public void Initialize(GridManager manager, Connectable origin)
        {
            m_Manager = manager;
            m_Vertices.Clear();
            m_Normals.Clear();
            m_UVs.Clear();
            m_Triangles.Clear();
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

            var mesh = gameObject.AddComponent<MeshFilter>().mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();

            renderer.materials = manager.GridOverlayMaterials;
            renderer.material.color = origin.HighlightColor;

            mesh.vertices = m_Vertices.ToArray();
            mesh.normals = m_Normals.ToArray();
            mesh.uv = m_UVs.ToArray();
            mesh.triangles = m_Triangles.ToArray();

            transform.SetParent(origin.transform, false);
            transform.localPosition += new Vector3(0, -0.005f, 0);
            transform.localEulerAngles = new Vector3(0, 180, 0);

            transform.localScale = m_TileScale;
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

            AddMeshGeometry(pos);
            return tile;
        }

        private void AddMeshGeometry(Vector3Int pos)
        {
            // Vertices Indices
            int v000 = m_Vertices.Count;
            int v001 = m_Vertices.Count + 1;
            int v101 = m_Vertices.Count + 2;
            int v100 = m_Vertices.Count + 3;
            int v010 = m_Vertices.Count + 4;
            int v011 = m_Vertices.Count + 5;
            int v111 = m_Vertices.Count + 6;
            int v110 = m_Vertices.Count + 7;

            // Vertices Coords
            m_Vertices.Add(pos + new Vector3(-0.5f, 0, -0.5f));
            m_Vertices.Add(pos + new Vector3(-0.5f, 0, 0.5f));
            m_Vertices.Add(pos + new Vector3(0.5f, 0, 0.5f));
            m_Vertices.Add(pos + new Vector3(0.5f, 0, -0.5f));
            m_Vertices.Add(pos + new Vector3(-0.5f, -0.01f, -0.5f));
            m_Vertices.Add(pos + new Vector3(-0.5f, -0.01f, 0.5f));
            m_Vertices.Add(pos + new Vector3(0.5f, -0.01f, 0.5f));
            m_Vertices.Add(pos + new Vector3(0.5f, -0.01f, -0.5f));

            // Normals by vertex
            // each are pointing away from the center of the tile
            m_Normals.Add(new Vector3(-1, 1, -1));
            m_Normals.Add(new Vector3(-1, 1, 1));
            m_Normals.Add(new Vector3(1, 1, 1));
            m_Normals.Add(new Vector3(1, 1, -1));
            m_Normals.Add(new Vector3(-1, -1, -1));
            m_Normals.Add(new Vector3(-1, -1, 1));
            m_Normals.Add(new Vector3(1, -1, 1));
            m_Normals.Add(new Vector3(1, -1, -1));

            // UVs (Texture Coords)
            m_UVs.Add(new Vector2(0, 0));
            m_UVs.Add(new Vector2(0, 1));
            m_UVs.Add(new Vector2(1, 1));
            m_UVs.Add(new Vector2(1, 0));
            m_UVs.Add(new Vector2(0, 0));
            m_UVs.Add(new Vector2(0, 1));
            m_UVs.Add(new Vector2(1, 1));
            m_UVs.Add(new Vector2(1, 0));

            // Top Face
            m_Triangles.Add(v000);
            m_Triangles.Add(v001);
            m_Triangles.Add(v101);
            m_Triangles.Add(v000);
            m_Triangles.Add(v101);
            m_Triangles.Add(v100);

            // Bottom Face
            m_Triangles.Add(v111);
            m_Triangles.Add(v011);
            m_Triangles.Add(v010);
            m_Triangles.Add(v110);
            m_Triangles.Add(v111);
            m_Triangles.Add(v010);
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
    }
}
