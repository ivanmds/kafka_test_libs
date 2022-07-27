using System.Text;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumerHelper
    {
        private const int DefaultMaxPollIntervalMs = 300000;

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
            var maxPollIntervalMs = listenerConfiguration.RetryTime is null ? DefaultMaxPollIntervalMs
                : listenerConfiguration.RetryTime.GetMilliseconds + DefaultMaxPollIntervalMs;

            return new ConsumerConfig
            {
                GroupId = listenerConfiguration.GroupId,
                BootstrapServers = listenerConfiguration.KafkaBuilder.KafkaConnection.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                MaxPollIntervalMs = maxPollIntervalMs
            };
        }

        internal static string GetTopicNameSkipped(string groupId, string currentTopicName)
        {
            return $"skipped.{groupId}.{currentTopicName}";
        }
        
        internal static string GetTopicNameDeadLetter(string groupId, string currentTopicName)
        {
            return $"dlq.{groupId}.{currentTopicName}";
        }
    }
}
