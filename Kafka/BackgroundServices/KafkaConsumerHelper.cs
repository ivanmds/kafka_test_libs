using System.Text;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumerHelper
    {
        private const int DEFAULT_MAXPOLL_INTERVALSMS = 300000;

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
            var maxPollIntervalMs = listenerConfiguration.RetryTime is null ? DEFAULT_MAXPOLL_INTERVALSMS
                : listenerConfiguration.RetryTime.GetMilliseconds + DEFAULT_MAXPOLL_INTERVALSMS;

            return new ConsumerConfig
            {
                GroupId = listenerConfiguration.GroupId,
                BootstrapServers = listenerConfiguration.KafkaBuilder.KafkaConnection.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                MaxPollIntervalMs = maxPollIntervalMs
            };
        }
    }
}
