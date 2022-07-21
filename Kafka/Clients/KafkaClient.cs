using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Configuration;
using Kafka.Values;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Kafka.Clients
{
    public class KafkaClient : IKafkaClient
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            },
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                    new StringEnumConverter(),
                    new KeyValuePairConverter()
            }
        };

        private readonly IProducer<string, string> _kafkaProducer;

        public KafkaClient(KafkaConnection kafkaConnection)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaConnection.BootstrapServers,
                SecurityProtocol = kafkaConnection.IsPlaintext ? SecurityProtocol.Plaintext : SecurityProtocol.Ssl
            };

            _kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task ProduceAsync(string topicName, string key, object message, CancellationToken cancellationToken)
            => await ProduceAsync(topicName, key, message, null, cancellationToken);

        public async Task ProduceAsync(string topicName, string key, object message, HeaderValue? header, CancellationToken cancellationToken)
        {
            var messageNotification = JsonConvert.SerializeObject(message, _settings);
            var kafkaMessage = new Message<string, string> { Key = key, Value = messageNotification };

            if (header != null)
            {
                kafkaMessage.Headers = new Headers();

                foreach (var kv in header.GetKeyValues())
                {
                    var valueBytes = Encoding.ASCII.GetBytes(kv.Value);
                    var msgHeader = new Header(kv.Key, valueBytes);
                    kafkaMessage.Headers.Add(msgHeader);
                }
            }

            await _kafkaProducer.ProduceAsync(topicName, kafkaMessage, cancellationToken);
        }
    }
}
