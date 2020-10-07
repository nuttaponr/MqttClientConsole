using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MqttClientConsole
{
    class Program
    {
        private static IManagedMqttClient managedMqttClientSubscriber;
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();

            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };

            var options = new MqttClientOptions
            {
                ClientId = "Note",
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = "xx.xx.xx.xx",
                    Port = 1883,
                    TlsOptions = tlsOptions
                }
            };

            if (options.ChannelOptions == null)
            {
                throw new InvalidOperationException();
            }

            options.Credentials = new MqttClientCredentials
            {
                Username = "device",
                Password = Encoding.UTF8.GetBytes("123456")
            };

            var topicFilter = new MqttTopicFilter { Topic = "esp32/dht/temperature" };


            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(5);

            managedMqttClientSubscriber = mqttFactory.CreateManagedMqttClient();
            managedMqttClientSubscriber.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnSubscriberConnected);
            managedMqttClientSubscriber.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnSubscriberDisconnected);
            managedMqttClientSubscriber.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);

            await managedMqttClientSubscriber.SubscribeAsync(topicFilter);
            await managedMqttClientSubscriber.StartAsync(
                 new ManagedMqttClientOptions
                 {
                     ClientOptions = options
                 });

            while (true)
            {

            }
        }

        private static void OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs x)
        {
            var item = $"Timestamp: {DateTime.Now:O} | Topic: {x.ApplicationMessage.Topic} | Payload: {x.ApplicationMessage.ConvertPayloadToString()} | QoS: {x.ApplicationMessage.QualityOfServiceLevel}";
            Console.WriteLine(item);
        }

        private static void OnSubscriberDisconnected(MqttClientDisconnectedEventArgs x)
        {
            Console.WriteLine("Subscriber Disconnected", "ConnectHandler");
        }

        private static void OnSubscriberConnected(MqttClientConnectedEventArgs x)
        {
            Console.WriteLine("Subscriber Connected", "ConnectHandler");
        }
    }
}
