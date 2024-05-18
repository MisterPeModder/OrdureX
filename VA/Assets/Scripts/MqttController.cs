using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Threading.Tasks;
using System;
using System.Threading;
using UnityEditor;

namespace OrdureX.Mqtt
{

    public class MqttController : MonoBehaviour
    {
        [Header("Connection Settings")]
        public string ServerUri = "localhost:9001/mqtt";
        [Tooltip("Connection/disconnection timeout in milliseconds")]
        public int ConnectionTimeout = 2000;
        [Tooltip("Duration to listen for messages in milliseconds")]
        public int ListenDuration = 2500;

        /// <summary>
        /// Root cancellation token for the MQTT task.
        /// Cancelled when the object is destroyed or when the play mode is exited.
        /// </summary>
        private CancellationTokenSource cts;

        void Start()
        {
            EditorApplication.playModeStateChanged += OnPlayStateChanged;

            Task.Run(async delegate
            {
                cts = new CancellationTokenSource();

                try
                {
                    await PingMqttServer();
                }
                catch (OperationCanceledException)
                {
                    // ignore cancellation
                    Debug.Log("Task cancelled");
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

        void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            Debug.Log("OnDestroy called, cancelling MQTT task");
            cts?.Cancel();
        }

        private void OnPlayStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("Play mode exited, cancelling MQTT task");
                cts?.Cancel();
            }
        }

        /// <summary>
        /// Connect to the MQTT server and send a message to the "ordurex/test/first" topic.
        /// </summary>
        private async Task PingMqttServer()
        {
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();

            // Connect to server using WebSocket because Unity rejects raw TCP for unknown reasons
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithWebSocketServer(o => o.WithUri(ServerUri))
                .Build();

            // Setup message handler *before* connecting
            mqttClient.ApplicationMessageReceivedAsync += (MqttApplicationMessageReceivedEventArgs args) =>
            {
                var message = args.ApplicationMessage;
                Debug.Log("Received message");
                Debug.Log($"- Topic: {message.Topic}");
                Debug.Log($"- QoS:   {message.QualityOfServiceLevel}");
                return Task.CompletedTask;
            };

            Debug.Log("Connecting to broker...");

            using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token))
            {
                timeoutCts.CancelAfter(ConnectionTimeout);
                await mqttClient.ConnectAsync(mqttClientOptions, timeoutCts.Token);
            }

            Debug.Log($"Connection established, listening for events (max: {ListenDuration}ms)...");

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("ordurex/test/first"))
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, cts.Token);

            await Task.Delay(ListenDuration / 2);

            var toPublish = new MqttApplicationMessageBuilder()
                .WithTopic("ordurex/test/first")
                .WithPayload("yes")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();
            await mqttClient.PublishAsync(toPublish, cts.Token);

            await Task.Delay(ListenDuration / 2);

            Debug.Log("Duration elapsed, disconnecting...");

            var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
                .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                .Build();

            using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token))
            {
                timeoutCts.CancelAfter(ConnectionTimeout);
                await mqttClient.DisconnectAsync(disconnectOptions, timeoutCts.Token);
            }

            Debug.Log("All done!");
        }
    }

}
