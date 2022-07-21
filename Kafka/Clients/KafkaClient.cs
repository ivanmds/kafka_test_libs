﻿using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Configuration;
using Kafka.DefaultValues;
using Kafka.Values;
using Newtonsoft.Json;

namespace Kafka.Clients
{
    public class KafkaClient : IKafkaClient
    {
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
            var messageNotification = JsonConvert.SerializeObject(message, DefaultSerializerSettings.JsonSettings);
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
