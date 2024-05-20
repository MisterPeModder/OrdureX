using MQTTnet;
using OrdureX.Mqtt;
using UnityEngine;


namespace OrdureX
{
    /// <summary>
    /// Test object for MQTT communication.
    /// Sends messages periodically and logs received messages.
    /// </summary>
    public class MqttTestObject : MonoBehaviour
    {
        public MqttController Controller;
        public int UpdateRate = 100;

        private TopicSubscription subscription;

        void OnDestroy()
        {
            subscription?.Unsubscribe();
        }

        public void MqttConnected()
        {
            subscription = Controller.Subscribe("ordurex/test", (args) =>
            {
                Debug.Log($"Received message: {args.ApplicationMessage.ConvertPayloadToString()}, topic: {args.ApplicationMessage.Topic}, QoS: {args.ApplicationMessage.QualityOfServiceLevel}, Retain: {args.ApplicationMessage.Retain}");
            });
        }

        private int counter = 0;

        public void FixedUpdate()
        {
            if (subscription == null)
            {
                return;
            }

            ++counter;
            if (counter >= UpdateRate)
            {
                counter = 0;
                var toPublish = new MqttApplicationMessageBuilder()
                    .WithTopic("ordurex/test")
                    .WithPayload("yes")
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build();
                Debug.Log("Publishing message...");
                Controller.Publish(toPublish);
            }
        }
    }
}
