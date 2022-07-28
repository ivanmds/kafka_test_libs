using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class ListenerConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly string _topicName;
        private string _sourceTopicName;
        private readonly string _groupId;
        private readonly KafkaBuilder _kafkaBuilder;
        private readonly RetryConfiguration? _retryConfiguration;
        private readonly RetryTime? _retryTime;

        public ListenerConfiguration(IServiceCollection services, string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration, RetryTime? retryTime = null)
        {
            _services = services;
            _topicName = topicName;
            _sourceTopicName = topicName;
            _groupId = groupId;
            _kafkaBuilder = kafkaBuilder;
            _retryConfiguration = retryConfiguration;
            _retryTime = retryTime;
        }

        internal static ListenerConfiguration Create(IServiceCollection services, string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration, RetryTime? retryTime = null)
            => new ListenerConfiguration(services, topicName, groupId, kafkaBuilder, retryConfiguration, retryTime);

        internal string TopicName => _topicName;
        internal string SourceTopicName => _sourceTopicName;
        internal string GroupId => _groupId;
        internal KafkaBuilder KafkaBuilder => _kafkaBuilder;
        internal RetryConfiguration? RetryConfiguration => _retryConfiguration;
        internal RetryTime? RetryTime => _retryTime;


        public ListenerConfiguration AddConsumer<TConsumer>(string eventName = DefaultHeader.KeyDefaultEvenName)
            where TConsumer : IConsumerMessage
        {
            var consumerKey = GetConsumerKey(GroupId, eventName);
            var consumerType = typeof(TConsumer);

            //Add validation when add twice the same consumer
            RegistryTypes.Register(consumerKey, consumerType);
            _services.AddSingleton(consumerType);

            return this;
        }

        internal string GetConsumerKey(string eventName)
          => GetConsumerKey(_groupId, eventName);

        internal string GetEventNameFromConsumerKey(string keyConsumer)
            => keyConsumer.Replace(_groupId, "");

        internal void SetSourceTopicName(string sourceTopicName)
            => _sourceTopicName = sourceTopicName;


        internal static string GetConsumerKey(string groupId, string eventName)
        {
            var sufixName = eventName;

            if (string.IsNullOrWhiteSpace(sufixName))
                sufixName = DefaultHeader.KeyDefaultEvenName;

            return $"{groupId}#{sufixName}";
        }
    }
}