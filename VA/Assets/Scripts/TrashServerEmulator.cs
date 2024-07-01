using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using OrdureX.Mqtt;
using UnityEditor;
using UnityEngine;

using static OrdureX.OrdureXEvents;

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
        [SerializeField]
        private MqttController m_Controller;

        [Tooltip("Whether to show logs in the Unity console.")]
        public bool ShowLogs = true;

        /// <summary>
        /// Root cancellation token for the MQTT task.
        /// Cancelled when the object is destroyed or when the play mode is exited.
        /// </summary>
        private CancellationTokenSource cts;

        // Simulation Variables //////////////////////////////////////////////
        private ArduinoStatus status;
        private bool trash0LidOpen;
        private bool trash2LidOpen;
        private SettingsManager m_SettingsManager;

        private void Awake()
        {
            m_SettingsManager = FindObjectOfType<SettingsManager>();
        }

        // Start is called before the first frame update
        void OnEnable()
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
                    ResetSimulation();
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

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
#endif
            cts?.Cancel();
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

        private void ResetSimulation()
        {
            status = ArduinoStatus.Stopped;
            trash0LidOpen = false;
            trash2LidOpen = false;
        }

        // public void Update()
        // {
        // }

        private async Task StartEmulator()
        {
            Debug.Log("Starting Trash Server Emulator.");

            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();

            // Setup message handler *before* connecting
            mqttClient.ApplicationMessageReceivedAsync += args => OnMessageReceived(mqttClient, args);

            await ConnectToMqttServer(mqttClient);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic($"{ACTION_NAMESPACE}/#"))
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
                .WithWebSocketServer(o => o.WithUri(m_SettingsManager.BrokerURL))
                .Build();

            var delay = m_Controller.DelayBetweenAttempts;

            for (int i = 0; i < m_Controller.MaxConnectionAttempts; i++)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                timeoutCts.CancelAfter(m_Controller.ConnectionTimeout);

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
                    Debug.LogWarning($"Failed to connect to MQTT server. Retrying in {delay}ms. ({i + 1}/{m_Controller.MaxConnectionAttempts})");
                    await Task.Delay(delay, cts.Token);
                    delay *= 2;
                }
            }

        }

        private int GetTrashId(string topic)
        {
            var prefix = $"{ACTION_NAMESPACE}/trash-";
            if (topic.StartsWith(prefix))
            {
                return int.Parse(topic.Substring(prefix.Length, 1));
            }
            return -1;
        }

        private void Log(string message)
        {
            if (ShowLogs)
            {
                Debug.Log("Emulator: " + message);
            }
        }

        private void LogError(string message)
        {
            if (ShowLogs)
            {
                Debug.LogError("Emulator: " + message);
            }
        }

        private async Task OnMessageReceived(IMqttClient mqttClient, MqttApplicationMessageReceivedEventArgs args)
        {
            var message = args.ApplicationMessage;
            var payload = message.PayloadSegment;
            var trash = GetTrashId(message.Topic);

            // echo back the simulation status
            if (message.Topic == $"{ACTION_NAMESPACE}/simulation")
            {
                if (payload.Array == null)
                {
                    LogError("Received empty message payload for status change");
                    return;
                }

                if (payload.Count != 17)
                {
                    LogError($"Invalid payload length for simulation status change, expected 17 bytes, got {payload.Count}");
                    return;
                }

                var newStatus = (ArduinoStatus)payload[0];

                if (newStatus == status)
                {
                    return;
                }
                status = newStatus;
                var clientUuid = new Guid(payload[1..17]);

                // Send back the status change
                var payloadBytes = new byte[17];

                payloadBytes[0] = (byte)newStatus;
                clientUuid.ToByteArray().CopyTo(payloadBytes, 1);

                await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"{STATUS_NAMESPACE}/simulation")
                    .WithPayload(payloadBytes)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build());
            }


            if (status != ArduinoStatus.Running)
            {
                return;
            }

            if (message.Topic == $"{ACTION_NAMESPACE}/trash-0/lid" || message.Topic == $"{ACTION_NAMESPACE}/trash-2/lid")
            {
                if (payload.Array == null)
                {
                    LogError("Received empty code payload for trash collection request");
                    return;
                }
                if (payload.Count != 1)
                {
                    LogError($"Invalid payload length for trash lid request, expected 1 byte, got {payload.Count}");
                    return;
                }
                var open = payload[0] == 1;

                if (trash == 0)
                {
                    trash0LidOpen = open;
                }
                else if (trash == 2)
                {
                    trash2LidOpen = open;
                }
                Log($"Received lid status for trash {trash}: {open}");

                await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"{STATUS_NAMESPACE}/trash-{trash}/lid")
                    .WithPayload(new byte[1] { open ? (byte)1 : (byte)0 })
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());
            }

            if (message.Topic == $"{ACTION_NAMESPACE}/trash-0/request-collect"
                || message.Topic == $"{ACTION_NAMESPACE}/trash-1/request-collect"
                || message.Topic == $"{ACTION_NAMESPACE}/trash-2/request-collect")
            {
                if (payload.Array == null)
                {
                    LogError("Received empty code payload for trash collection request");
                    return;
                }

                if (payload.Count < 16)
                {
                    LogError($"Invalid payload length for simulation status change, expected at least 16 bytes, got {payload.Count}");
                    return;
                }
                var clientUuid = new Guid(payload[0..16]);
                var code = System.Text.Encoding.UTF8.GetString(payload.Array, payload.Offset + 16, payload.Count - 16);

                Log($"Received trash {trash} collection request from {clientUuid} with code: {code}");

                await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"{STATUS_NAMESPACE}/trash-{trash}/collect-requested")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());
            }

            if (message.Topic == $"{ACTION_NAMESPACE}/trash-1/buzzer")
            {
                if (payload.Array == null)
                {
                    LogError("Received empty code payload for trash collection request");
                    return;
                }
                if (payload.Count != 1)
                {
                    LogError($"Invalid payload length for trash buzzer request, expected 1 byte, got {payload.Count}");
                    return;
                }
                var sound = payload[0];

                Log($"Received buzzer request for trash {trash} with sound: {sound}");
            }

            if (message.Topic == $"{ACTION_NAMESPACE}/trash-2/display")
            {
                if (payload.Array == null)
                {
                    Debug.LogError("Received empty code payload for trash collection request");
                    return;
                }
                var text = System.Text.Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);

                Log($"Received display request for trash {trash} with text: {text}");
            }
        }
    }

    public enum ArduinoStatus : byte
    {
        Stopped = 0,
        Running = 1,
        Paused = 2,
    }
}
