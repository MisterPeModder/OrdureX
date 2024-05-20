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

        // Start is called before the first frame update
        void Start()
        {
            subscription = Controller.Subscribe("ordurex/test", (args) =>
            {
                Debug.Log($"Received message: {args.ApplicationMessage.ConvertPayloadToString()}, topic: {args.ApplicationMessage.Topic}, QoS: {args.ApplicationMessage.QualityOfServiceLevel}, Retain: {args.ApplicationMessage.Retain}");
            });
        }

        void OnDestroy()
        {
            subscription.Unsubscribe();
        }

        private int counter = 0;

        public void FixedUpdate()
        {
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
