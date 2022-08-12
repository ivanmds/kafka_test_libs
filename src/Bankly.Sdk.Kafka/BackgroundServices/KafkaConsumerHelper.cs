using System.Text;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumerHelper
    {
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
                AutoOffsetReset = AutoOffsetReset.Latest,
                //EnableAutoCommit = false,
                AutoCommitIntervalMs = 500,
                MaxPollIntervalMs = maxPollIntervalMs
            };
        }
    }
}
