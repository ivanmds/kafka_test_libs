using System.Text;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Exceptions;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumerHelper
    {
        private static CachedSchemaRegistryClient _cachedSchemaRegistryClient = null;
        private const int DEFAULT_MAX_POLL_INTERVALS_MS = 300000;

        internal static HeaderValue ParseHeader(Headers headers)
        {
            var headerValue = HeaderValue.Create();

            if (headers is null)
                return headerValue;

            foreach (var kv in headers)
            {
                var value = Encoding.Default.GetString(kv.GetValueBytes());
                headerValue.PutKeyValue(kv.Key, value);
            }

            return headerValue;
        }

        internal static ConsumerConfig GetConsumerConfig(ListenerConfiguration listenerConfiguration)
        {
            var maxPollIntervalMs = listenerConfiguration.RetryTime is null ? DEFAULT_MAX_POLL_INTERVALS_MS
                : listenerConfiguration.RetryTime.GetMilliseconds + DEFAULT_MAX_POLL_INTERVALS_MS;

            var kafkaConnection = listenerConfiguration.KafkaBuilder.KafkaConnection;
            return new ConsumerConfig
            {
                GroupId = listenerConfiguration.GroupId,
                BootstrapServers = kafkaConnection.BootstrapServers,
                SecurityProtocol = kafkaConnection.IsPlaintext ? SecurityProtocol.Plaintext : SecurityProtocol.Ssl,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                MaxPollIntervalMs = maxPollIntervalMs
            };
        }

        internal static CachedSchemaRegistryClient GetCachedSchemaRegistryClient(ListenerConfiguration listenerConfiguration)
        {
            if (_cachedSchemaRegistryClient == null)
            {
                var kafkaConnection = listenerConfiguration.KafkaBuilder.KafkaConnection;

                if (string.IsNullOrEmpty(kafkaConnection.UrlSchemaRegistryServer))
                    throw new ConnectionSchemaRegistryServerException();

                _cachedSchemaRegistryClient = new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = kafkaConnection.UrlSchemaRegistryServer
                });
            }

            return _cachedSchemaRegistryClient;
        }
    }
}
