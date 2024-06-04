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

        SurfaceTileManager()
        {
            Tiles = m_Tiles;
        }

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

        void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

        private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var newImage in eventArgs.added)
            {
                var newTile = newImage.GetComponent<SurfaceTile>();
                newTile.Activate(this, newImage.referenceImage.name);
                m_Tiles.Add(newTile);
            }

            foreach (var removedImage in eventArgs.removed)
            {
                var removedTile = removedImage.GetComponent<SurfaceTile>();
                m_Tiles.Remove(removedTile);
            }
        }
    }
}
