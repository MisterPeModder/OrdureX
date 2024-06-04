using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace OrdureX.AR
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class SurfaceTileManager : MonoBehaviour
    {
        ARTrackedImageManager m_TrackedImageManager;

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
                newTile.Activate(newImage.referenceImage.name);
            }
        }
    }
}
