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
        public SimulationStatus Status = SimulationStatus.Stopped;
        public bool Connected = false;

        private Guid clientUuid;


        // Start is called before the first frame update
        public void Start()
        {
            Status = SimulationStatus.Stopped;
            clientUuid = Guid.NewGuid();
            ClientId = clientUuid.ToString();
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
            Connected = true;
        }

        public void MqttDisconnected()
        {
            Connected = false;
        }

        // Update is called once per frame
        public void Update()
        {

        }

        // Unity UI Bindings //////////////////////////////////////////////////

        public void OnStartOrStop()
        {
            if (!Connected)
            {
                Debug.LogError("Cannot start/stop: not connected to MQTT broker");
                return;
            }

            var newStatus = Status == SimulationStatus.Stopped || Status == SimulationStatus.Paused
                ? SimulationStatus.Running
                : SimulationStatus.Stopped;

            var payloadBytes = new byte[17];

            payloadBytes[0] = (byte)newStatus;
            clientUuid.ToByteArray().CopyTo(payloadBytes, 1);

            Debug.Log("Publishing status change: " + newStatus);

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

            if (!Enum.IsDefined(typeof(SimulationStatus), statusByte))
            {
                Debug.LogError($"Invalid status change byte: {statusByte}");
                return;
            }

            var newStatus = (SimulationStatus)statusByte;
            var uuid = new Guid(uuidBytes);

            Debug.Log("Received status change: " + newStatus + " by client " + uuid);

            Status = newStatus;
        }
    }

    public enum SimulationStatus : byte
    {
        Stopped = 0,
        Running = 1,
        Paused = 2,
    }

}
