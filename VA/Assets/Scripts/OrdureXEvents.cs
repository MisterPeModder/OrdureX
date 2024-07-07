using MQTTnet;
using MQTTnet.Client;
using OrdureX.Mqtt;
using UnityEngine;
using MQTTnet.Protocol;
using System;

namespace OrdureX
{
    public class OrdureXEvents : MonoBehaviour
    {
        public MqttController Controller;

        [Header("Runtime values (read-only)")]
        [Tooltip("Client UUID for this Unity instance, leave empty to generate a new one")]
        public string ClientId = "";

        public Guid ClientUuid;

        public Action<bool> OnTrash0CollectRequested { get; set; }
        public Action<bool> OnTrash1CollectRequested { get; set; }
        public Action<bool> OnTrash2CollectRequested { get; set; }

        public Action<Guid> OnTrash0InvalidCode { get; set; }
        public Action<Guid> OnTrash1InvalidCode { get; set; }
        public Action<Guid> OnTrash2InvalidCode { get; set; }

        public Action<bool> OnTrash1BurningChanged { get; set; }

        public Action<bool> OnTrash0LidChanged { get; set; }
        public Action<bool> OnTrash2LidChanged { get; set; }

        private SimulationStateManager m_SimulationStateManager;

        public const string ACTION_NAMESPACE = "ordurex/action";
        public const string STATUS_NAMESPACE = "ordurex/status";

        private void Awake()
        {
            m_SimulationStateManager = FindObjectOfType<SimulationStateManager>();
        }


        // Start is called before the first frame update
        public void Start()
        {
            ClientUuid = Guid.NewGuid();
            ClientId = ClientUuid.ToString();
            Controller.OnConnected.AddListener(MqttConnected);
            Controller.OnDisconnected.AddListener(MqttDisconnected);
        }

        public void OnDestroy()
        {
            Controller.OnConnected.RemoveListener(MqttConnected);
            Controller.OnDisconnected.RemoveListener(MqttDisconnected);
        }


        public void MqttConnected()
        {
            Controller.Subscribe($"{STATUS_NAMESPACE}/simulation", DecodeStatusChange);
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-0/collect-requested", (_args) => OnTrash0CollectRequested.Invoke(true));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-1/collect-requested", (_args) => OnTrash1CollectRequested.Invoke(true));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-2/collect-requested", (_args) => OnTrash2CollectRequested.Invoke(true));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-0/invalid-code", (args) => DecodeInvalidCode(args, OnTrash0InvalidCode));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-1/invalid-code", (args) => DecodeInvalidCode(args, OnTrash1InvalidCode));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-2/invalid-code", (args) => DecodeInvalidCode(args, OnTrash2InvalidCode));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-1/burning", (args) => DecodeBoolEvent(args, OnTrash1BurningChanged));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-0/lid", (args) => DecodeBoolEvent(args, OnTrash0LidChanged));
            Controller.Subscribe($"{STATUS_NAMESPACE}/trash-2/lid", (args) => DecodeBoolEvent(args, OnTrash2LidChanged));
        }

        public void MqttDisconnected()
        {
        }

        // Update is called once per frame
        public void Update()
        {

        }

        // Unity UI Bindings //////////////////////////////////////////////////

        public void OnStartOrStop()
        {
            var currentStatus = m_SimulationStateManager.Status;

            byte newStatusByte = currentStatus switch
            {
                SimulationStatus.Stopped => 1,
                SimulationStatus.Paused => 1,
                SimulationStatus.Running => 0,
                _ => throw new InvalidOperationException($"Cannot start/stop: status is neither Stopped, Paused, nor Running (is {currentStatus})")
            };

            var payloadBytes = new byte[17];

            payloadBytes[0] = newStatusByte;
            ClientUuid.ToByteArray().CopyTo(payloadBytes, 1);

            Debug.Log("Publishing status change: " + newStatusByte);

            Controller.Publish(new MqttApplicationMessageBuilder()
                .WithTopic("ordurex/action/simulation")
                .WithPayload(payloadBytes)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build());
        }

        // Manual Event Triggers //////////////////////////////////////////////

        public void SetCollectRequested(int trashIndex, bool value)
        {
            if (trashIndex == 0)
            {
                OnTrash0CollectRequested.Invoke(value);
            }
            else if (trashIndex == 1)
            {
                OnTrash1CollectRequested.Invoke(value);
                if (!value)
                {
                    OnTrash1BurningChanged.Invoke(false);
                }
            }
            else if (trashIndex == 2)
            {
                OnTrash2CollectRequested.Invoke(value);
            }
        }

        // Events Callbacks ///////////////////////////////////////////////////

        private void DecodeStatusChange(MqttApplicationMessageReceivedEventArgs args)
        {
            var message = args.ApplicationMessage;
            var payload = message.PayloadSegment;

            if (payload.Array == null)
            {
                Debug.LogError("Received empty message payload for status change");
                return;
            }

            if (payload.Count != 17)
            {
                Debug.LogError($"Expected {17} bytes in status change payload, got {payload.Array.Length}");
                return;
            }

            var statusByte = payload.Array[payload.Offset];
            var uuidBytes = new byte[16];
            payload.Slice(1, 16).CopyTo(uuidBytes);

            if (!DecodeStatus(statusByte, out var newStatus))
            {
                Debug.LogError($"Invalid status change byte: {statusByte}");
                return;
            }

            var uuid = new Guid(uuidBytes);

            Debug.Log("Received status change: " + newStatus + " by client " + uuid);

            var currentStatus = m_SimulationStateManager.Status;

            if (currentStatus != SimulationStatus.Stopped && currentStatus != SimulationStatus.Paused && currentStatus != SimulationStatus.Running)
            {
                Debug.LogError($"Cannot change status to {newStatus}: current status is {currentStatus} (expected Stopped, Paused, or Running)");
                return;
            }

            m_SimulationStateManager.Status = newStatus;
        }


        private bool DecodeStatus(byte statusByte, out SimulationStatus status)
        {
            switch (statusByte)
            {
                case 0:
                    status = SimulationStatus.Stopped;
                    return true;
                case 1:
                    status = SimulationStatus.Running;
                    return true;
                case 2:
                    status = SimulationStatus.Paused;
                    return true;
                default:
                    status = SimulationStatus.ConnectionFailed;
                    return false;
            }
        }

        private void DecodeInvalidCode(MqttApplicationMessageReceivedEventArgs args, Action<Guid> next)
        {
            var message = args.ApplicationMessage;
            var payload = message.PayloadSegment;

            if (payload.Array != null && payload.Count == 16)
            {
                var uuid = new Guid(payload.Array);
                next.Invoke(uuid);
            }
            else
            {
                next.Invoke(Guid.Empty);
            }
        }

        private void DecodeBoolEvent(MqttApplicationMessageReceivedEventArgs args, Action<bool> next)
        {
            var message = args.ApplicationMessage;
            var payload = message.PayloadSegment;

            if (payload.Array != null && payload.Count == 1)
            {
                var value = payload.Array[payload.Offset] == 1;
                next.Invoke(value);
            }
            else
            {
                next.Invoke(false);
            }
        }
    }

}
