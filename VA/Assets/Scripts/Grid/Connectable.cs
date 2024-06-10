using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace OrdureX.Grid
{
    public class Connectable : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Reference to the GridManager in the scene. Automatically found if not set.")]
        public GridManager GridManager;

        public ISet<Connectable> AllTouching;
        public ISet<Connectable> NextAllTouching;
        public Color HighlightColor;

        [Header("Connectors")]
        public Connector TopConnector;
        public Connector BottomConnector;
        public Connector RightConnector;
        public Connector LeftConnector;


        private void Start()
        {
            GridManager = FindObjectOfType<GridManager>();
            if (GridManager == null)
            {
                Debug.LogError("Failed to find GridManager in scene!");
            }

            AllTouching = null;
            NextAllTouching = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Clicked on " + name);
            GridManager.CreateGrid(this);
        }

        public void OnContactStart(Connector ownConnector, Connector otherConnector)
        {
            if (ownConnector.TouchingConnector != null || otherConnector.TouchingConnector != null)
            {
                // Connected to something else
                return;
            }

            if (IsTouching(otherConnector.Owner))
            {
                // Already connected to this
                return;
            }

            ownConnector.TouchingConnector = otherConnector;
            otherConnector.TouchingConnector = ownConnector;
        }

        public void OnContactEnd(Connector ownConnector, Connector otherConnector)
        {
            if (ownConnector.TouchingConnector == otherConnector && otherConnector.TouchingConnector == ownConnector)
            {
                ownConnector.TouchingConnector = null;
                otherConnector.TouchingConnector = null;
            }
        }

        private bool IsTouching(Connectable other)
        {
            return TopConnector.Touching == other || BottomConnector.Touching == other || RightConnector.Touching == other || LeftConnector.Touching == other;
        }

        public Connector GetConnector(Side side)
        {
            return side switch
            {
                Side.Top => TopConnector,
                Side.Bottom => BottomConnector,
                Side.Right => RightConnector,
                Side.Left => LeftConnector,
                _ => null
            };
        }


    }
}
