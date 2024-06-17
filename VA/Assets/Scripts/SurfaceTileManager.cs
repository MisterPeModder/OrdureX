using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace OrdureX.AR
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class SurfaceTileManager : MonoBehaviour
    {
        public IReadOnlyList<SurfaceTile> Tiles { get; private set; }
        private ARTrackedImageManager m_TrackedImageManager;
        private readonly List<SurfaceTile> m_Tiles = new();

        [SerializeField]
        private TileNameToPathPrefab[] m_TileNameToPathPrefab;

        private Dictionary<string, GameObject> m_NameToPrefab;

        SurfaceTileManager()
        {
            Tiles = m_Tiles;
        }

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            m_NameToPrefab = new Dictionary<string, GameObject>();
            foreach (var pair in m_TileNameToPathPrefab)
            {
                m_NameToPrefab[pair.Name] = pair.Prefab;
            }
        }

        void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

        void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

        private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var newImage in eventArgs.added)
            {
                var newTile = newImage.GetComponent<SurfaceTile>();

                if (!m_NameToPrefab.TryGetValue(newImage.referenceImage.name, out var pathPrefab))
                {
                    Debug.LogError($"No path prefab found for image '{newImage.referenceImage.name}'");
                    continue;
                }
                newTile.Activate(this, newImage.referenceImage.name, pathPrefab);
                m_Tiles.Add(newTile);
            }

            foreach (var removedImage in eventArgs.removed)
            {
                var removedTile = removedImage.GetComponent<SurfaceTile>();
                m_Tiles.Remove(removedTile);
            }
        }
    }

    [Serializable]
    public struct TileNameToPathPrefab
    {
        public string Name;
        public GameObject Prefab;
    }
}
