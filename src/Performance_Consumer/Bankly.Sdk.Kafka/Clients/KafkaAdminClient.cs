using Bankly.Sdk.Kafka.Configuration;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Bankly.Sdk.Kafka.Clients
{
    internal class KafkaAdminClient : IKafkaAdminClient, IDisposable
    {
        private bool _disposedValue;
        private readonly IAdminClient _adminClient;

        public KafkaAdminClient(KafkaConnection kafkaConnection)
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = kafkaConnection.BootstrapServers,
                SecurityProtocol = kafkaConnection.IsPlaintext ? SecurityProtocol.Plaintext : SecurityProtocol.Ssl
            };

            _adminClient = new AdminClientBuilder(config).Build();

            SettingNumOfPartitionAsync("bankly.account.customers.create_customer.request",
                                       "bankly.event.account.customers",
                                       "bankly.card.cards.create_card.request")
                                       .Wait();
        }

        public async Task PutTopicAsync(string topicName)
        {
            var metadata = _adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(10));
            if (metadata == null)
            {
                var topicSpecs = GetTopicSpecification(topicName);
                await _adminClient.CreateTopicsAsync(topicSpecs);
            }
        }

        private IEnumerable<TopicSpecification> GetTopicSpecification(params string[] topicNames)
        {
            foreach (var topic in topicNames)
            {
                var topicSpec = new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1
                };
                yield return topicSpec;
            }
        }

        private async Task SettingNumOfPartitionAsync(params string[] topicNames)
        {
            var partitionSpec = GetPartitionSpec(topicNames).ToList();

            foreach (var spec in partitionSpec.ToList())
            {
                var metadata = _adminClient.GetMetadata(spec.Topic, TimeSpan.FromSeconds(10));
                if (metadata == null || metadata.Topics[0].Partitions.Count() >= spec.IncreaseTo)
                    partitionSpec.Remove(spec);
            }

            if (partitionSpec?.Count > 0)
                await _adminClient.CreatePartitionsAsync(partitionSpec);
        }

        private IEnumerable<PartitionsSpecification> GetPartitionSpec(params string[] topicNames)
        {
            foreach (var topic in topicNames)
            {
                yield return new PartitionsSpecification { Topic = topic, IncreaseTo = 50 };
            }
        }

        ~KafkaAdminClient() => Dispose(true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _adminClient.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
