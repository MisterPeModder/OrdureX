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

        private SimulationStateManager m_SimulationStateManager;

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
            Controller.Subscribe("ordurex/status/simulation", OnStatusChange);
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

        // Events Callbacks ///////////////////////////////////////////////////

        private void OnStatusChange(MqttApplicationMessageReceivedEventArgs args)
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
    }

}
