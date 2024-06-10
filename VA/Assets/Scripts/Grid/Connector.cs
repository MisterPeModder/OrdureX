using UnityEngine;

namespace OrdureX.Grid
{
    [RequireComponent(typeof(Collider))]
    public class Connector : MonoBehaviour
    {
        public Connectable Owner;

        public Connectable Touching
        {
            get
            {
                if (TouchingConnector == null)
                {
                    return null;
                }
                return TouchingConnector.Owner;
            }
        }

        public Connector TouchingConnector { get; set; }

        public Side Side { get; private set; }

        private void Awake()
        {
            if (Owner.TopConnector == this)
            {
                Side = Side.Top;
            }
            else if (Owner.BottomConnector == this)
            {
                Side = Side.Bottom;
            }
            else if (Owner.RightConnector == this)
            {
                Side = Side.Right;
            }
            else if (Owner.LeftConnector == this)
            {
                Side = Side.Left;
            }
            else
            {
                Debug.LogError("Connector is not assigned to Connectable!");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Connector>(out var otherConnector))
            {
                Owner.OnContactStart(this, otherConnector);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<Connector>(out var otherConnector) && otherConnector == TouchingConnector)
            {
                Owner.OnContactEnd(this, otherConnector);
            }
        }

    }
}
