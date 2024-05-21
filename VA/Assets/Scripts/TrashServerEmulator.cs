using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using OrdureX.Mqtt;
using UnityEditor;
using UnityEngine;

namespace OrdureX
{
    /// <summary>Emulates the Arduino+ESP-01+Sensors combination for testing.
    /// <para>
    /// Uses a separate MQTT client to do it.
    /// </para>
    /// </summary>
    public class TrashServerEmulator : MonoBehaviour
    {
        [Tooltip("Used to get the URL of the MQTT broker.")]
        public MqttController Controller;

        /// <summary>
        /// Root cancellation token for the MQTT task.
        /// Cancelled when the object is destroyed or when the play mode is exited.
        /// </summary>
        private CancellationTokenSource cts;

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
#endif

            UnityThreadExecutor.Init();

            // Launch emulato  in a separate thread for Unity
            Task.Run(async delegate
            {
                cts = new CancellationTokenSource();

                try
                {
                    await StartEmulator();
                }
                catch (OperationCanceledException)
                {
                    // ignore cancellation
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, this);
                }
                finally
                {
                    cts.Dispose();
                    cts = null;
                }
            });

        }

#if UNITY_EDITOR
        private void OnPlayStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                cts?.Cancel();
            }
        }
#endif

        public void Update()
        {

        }

        private async Task StartEmulator()
        {
            Debug.Log("Starting Trash Server Emulator.");

            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();

            // Setup message handler *before* connecting
            mqttClient.ApplicationMessageReceivedAsync += args => OnMessageReceived(mqttClient, args);

            await ConnectToMqttServer(mqttClient);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("ordurex/action/simulation"))
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, cts.Token);

            Debug.Log("Trash Server Emulator started and ready.");

            cts.Token.WaitHandle.WaitOne();

            Debug.Log("Trash Server Emulator stopped.");
        }

        private async Task ConnectToMqttServer(IMqttClient mqttClient)
        {

            // Connect to server using WebSocket because Unity rejects raw TCP for unknown reasons
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithWebSocketServer(o => o.WithUri(Controller.ServerUri))
                .Build();

            var delay = Controller.DelayBetweenAttempts;

            for (int i = 0; i < Controller.MaxConnectionAttempts; i++)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                timeoutCts.CancelAfter(Controller.ConnectionTimeout);

                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, timeoutCts.Token);
                    return;
                }
                catch
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        // server is shutting down
                        throw;
                    }
                    Debug.LogWarning($"Failed to connect to MQTT server. Retrying in {delay}ms. ({i + 1}/{Controller.MaxConnectionAttempts})");
                    await Task.Delay(delay, cts.Token);
                    delay *= 2;
                }
            }

        }

        private async Task OnMessageReceived(IMqttClient mqttClient, MqttApplicationMessageReceivedEventArgs args)
        {
            var message = args.ApplicationMessage;

            // echo back the simulation status
            if (message.Topic == "ordurex/action/simulation")
            {
                var payload = message.PayloadSegment;

                if (payload.Array == null)
                {
                    Debug.LogError("Received empty message payload for status change");
                    return;
                }

                if (payload.Count < 17)
                {
                    Debug.LogError("Invalid payload length for simulation status change.");
                    return;
                }

                var newStatus = (SimulationStatus)payload[0];
                var clientUuid = new Guid(payload[1..17]);

                // Send back the status change
                var payloadBytes = new byte[17];

                payloadBytes[0] = (byte)newStatus;
                clientUuid.ToByteArray().CopyTo(payloadBytes, 1);

                await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic("ordurex/status/simulation")
                    .WithPayload(payloadBytes)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build());
            }
        }
    }
}
