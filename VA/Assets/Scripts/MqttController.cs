using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OrdureX.Mqtt
{
    /// <summary>Enacapsulates an MQTT client and provides methods to interact with it.
    /// <para>
    /// The MQTT client is run in a separate thread to avoid blocking the Unity main thread.
    /// This class handles transparently the communication between the Unity main thread and the MQTT client thread.
    /// </para>
    /// 
    /// <para>
    /// To use it, add a MqttController field to your component and set it in the Unity editor.
    /// Then call Subscribe() to listen for messages and Publish() to send messages.
    /// </para>
    /// </summary>
    public class MqttController : MonoBehaviour
    {
        [Header("Connection Settings")]
        public string ServerUri = "broker.hivemq.com:8000/mqtt";
        [Tooltip("Maximum number of connection attempts before giving up")]
        public int MaxConnectionAttempts = 3;
        [Tooltip("Initial delay between connection attempts in milliseconds, will increase exponentially with each attempt")]
        public int DelayBetweenAttempts = 500;
        [Tooltip("Connection/Disconnection timeout in milliseconds")]
        public int ConnectionTimeout = 1000;

        [Header("UI")]
        [Tooltip("Text field to display the connection status, optional")]
        public TextMeshProUGUI StatusDisplay;

        [Header("Events")]
        [Tooltip("Event triggered when the MQTT client is connected to the server")]
        public UnityEvent OnConnected;
        [Tooltip("Event triggered when the MQTT client is disconnected from the server")]
        public UnityEvent OnDisconnected;

        /// <summary>
        /// Root cancellation token for the MQTT task.
        /// Cancelled when the object is destroyed or when the play mode is exited.
        /// </summary>
        private CancellationTokenSource cts;

        private readonly Dictionary<string, List<TopicSubscription>> subscriptions = new();

        private readonly ConcurrentQueue<Func<IMqttClient, MqttFactory, Task>> mqttThreadActions = new();
        private readonly AutoResetEvent mqttThreadActionsSignal = new(true);

        public void Start()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
#endif

            UnityThreadExecutor.Init();

            // Launch MQTT client in a separate thread for Unity
            Task.Run(async delegate
            {
                cts = new CancellationTokenSource();

                try
                {
                    await StartMqttClient();
                    SetStatus("Stopped");
                }
                catch (OperationCanceledException)
                {
                    // ignore cancellation
                    Debug.Log("MQTT Task cancelled");
                    SetStatus("Cancelled");
                }
                catch (Exception ex)
                {
                    SetStatus($"Errored: {ex.Message}");
                    Debug.LogException(ex, this);
                }
                finally
                {
                    cts.Dispose();
                    subscriptions.Clear();
                    mqttThreadActionsSignal.Set();
                    cts = null;
                    OnDisconnected.Invoke();
                }
            });
        }

        public void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
#endif
            SetStatus("Cancelling");
            cts?.Cancel();
        }

        /// <summary>
        /// Subcribes to a given MQTT topic using the provided filter.
        /// </summary>
        /// <param name="topicFilter">The topic, accepts wildcards ('+', '#')</param>
        /// <param name="handler">The function to run on message reception</param>
        /// <returns>A handle to this subscription</returns>
        public TopicSubscription Subscribe(string topicFilter, Action<MqttApplicationMessageReceivedEventArgs> handler)
        {
            var subscription = new TopicSubscription(this, topicFilter, handler);

            ExecuteOnMqttThread(async (mqttClient, mqttFactory) =>
            {
                // We are on Unity's thread
                if (!subscriptions.ContainsKey(topicFilter))
                {
                    subscriptions[topicFilter] = new List<TopicSubscription>();

                    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(f => f.WithTopic(topicFilter))
                        .Build();

                    await mqttClient.SubscribeAsync(mqttSubscribeOptions, cts.Token);
                }
                subscriptions[topicFilter].Add(subscription);
            });

            return subscription;
        }

        /// <summary>
        /// Publishes a message to the MQTT server.
        /// It is safe to call this method from any thread.
        /// </summary>
        /// <param name="message">The MQTT message, use MqttApplicationMessageBuilder to construct.</param>
        public void Publish(MqttApplicationMessage message)
        {
            ExecuteOnMqttThread((mqttClient, mqttFactory) => mqttClient.PublishAsync(message, cts.Token));
        }

        internal void Unsubscribe(string topicFilter, TopicSubscription subscription)
        {
            if (subscriptions.ContainsKey(topicFilter))
            {
                subscriptions[topicFilter]?.Remove(subscription);
            }
        }

#if UNITY_EDITOR
        private void OnPlayStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                SetStatus("Cancelling");
                cts?.Cancel();
            }
        }
#endif

        private async Task StartMqttClient()
        {
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();

            // Setup message handler *before* connecting
            subscriptions.Clear();
            mqttThreadActionsSignal.Set();
            mqttClient.ApplicationMessageReceivedAsync += (MqttApplicationMessageReceivedEventArgs args) =>
            {
                var message = args.ApplicationMessage;
                var toInvoke = new HashSet<TopicSubscription>();

                // Collect matching subscriptions
                foreach (var (filter, subs) in subscriptions)
                {
                    if (MqttTopicFilterComparer.Compare(message.Topic, filter) == MqttTopicFilterCompareResult.IsMatch)
                    {
                        toInvoke.UnionWith(subs);
                    }
                }

                // Dispatch to Unity main thread
                SetStatus($"Listening, last topic: {message.Topic}");
                ExecuteOnUnityThread(() =>
                {
                    foreach (var sub in toInvoke)
                    {
                        sub.Invoke(args);
                    }
                });

                return Task.CompletedTask;
            };

            await ConnectToMqttServer(mqttClient);

            SetStatus("Listening");

            // Poll events from the Unity thread indefinitely
            while (!cts.Token.IsCancellationRequested)
            {
                mqttThreadActionsSignal.WaitOne(100);
                var toAwait = new List<Task>();

                while (mqttThreadActions.TryDequeue(out var action))
                {
                    toAwait.Add(action(mqttClient, mqttFactory));
                }
                await Task.WhenAll(toAwait);
            }


            SetStatus("Disconnecting");

            var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
                .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                .Build();

            using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token))
            {
                timeoutCts.CancelAfter(ConnectionTimeout);
                await mqttClient.DisconnectAsync(disconnectOptions, timeoutCts.Token);
            }
        }

        private async Task ConnectToMqttServer(IMqttClient mqttClient)
        {

            // Connect to server using WebSocket because Unity rejects raw TCP for unknown reasons
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithWebSocketServer(o => o.WithUri(ServerUri))
                .Build();

            var delay = DelayBetweenAttempts;

            for (int i = 0; i < MaxConnectionAttempts; i++)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                timeoutCts.CancelAfter(ConnectionTimeout);

                try
                {
                    SetStatus($"Connecting to MQTT server, attempt {i + 1}/{MaxConnectionAttempts}...");
                    await mqttClient.ConnectAsync(mqttClientOptions, timeoutCts.Token);
                    SetStatus("Connection established");
                    ExecuteOnUnityThread(() => OnConnected.Invoke());
                    return;
                }
                catch
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        // server is shutting down
                        throw;
                    }
                    SetStatus($"Connection failed, retrying in {delay}ms...");
                    await Task.Delay(delay, cts.Token);
                    delay *= 2;
                }
            }

        }

        private void SetStatus(string status)
        {
            ExecuteOnUnityThread(() =>
            {
                if (StatusDisplay != null)
                {
                    StatusDisplay.text = status;
                }
            });
        }

        private void ExecuteOnUnityThread(Action action)
        {
            UnityThreadExecutor.Execute(action);
        }

        private void ExecuteOnMqttThread(Func<IMqttClient, MqttFactory, Task> action)
        {
            mqttThreadActions.Enqueue(action);
            mqttThreadActionsSignal.Set();
        }
    }

    /// <summary>
    /// A subscription handle for a topic.
    /// </summary>
    public class TopicSubscription
    {
        private readonly MqttController controller;
        private readonly string topicFilter;
        private readonly Action<MqttApplicationMessageReceivedEventArgs> handler;

        internal TopicSubscription(MqttController controller, string topicFilter, Action<MqttApplicationMessageReceivedEventArgs> handler)
        {
            this.controller = controller;
            this.topicFilter = topicFilter;
            this.handler = handler;
        }

        public void Unsubscribe()
        {
            controller.Unsubscribe(topicFilter, this);
        }

        public void Invoke(MqttApplicationMessageReceivedEventArgs args)
        {
            handler(args);
        }
    }

}
