using System.Collections.Generic;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        private readonly List<string> _ignoreEvents;
        private readonly bool _useAvro;

        public ListenerConfiguration(
            IServiceCollection services,
            string topicName,
            string groupId,
            KafkaBuilder kafkaBuilder,
            RetryConfiguration? retryConfiguration,
            RetryTime? retryTime = null,
            bool useAvro = false)
        {
            _services = services;
            _topicName = topicName;
            _sourceTopicName = topicName;
            _groupId = groupId;
            _kafkaBuilder = kafkaBuilder;
            _retryConfiguration = retryConfiguration;
            _retryTime = retryTime;
            _ignoreEvents = new List<string>();
            _useAvro = useAvro;
        }

        internal static ListenerConfiguration Create(IServiceCollection services, string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration, RetryTime? retryTime = null, bool useAvro = false)
            => new ListenerConfiguration(services, topicName, groupId, kafkaBuilder, retryConfiguration, retryTime, useAvro);

        internal string TopicName => _topicName;
        internal string SourceTopicName => _sourceTopicName;
        internal string GroupId => _groupId;
        internal KafkaBuilder KafkaBuilder => _kafkaBuilder;
        internal RetryConfiguration? RetryConfiguration => _retryConfiguration;
        internal RetryTime? RetryTime => _retryTime;
        internal bool useAvro => _useAvro;


        public ListenerConfiguration AddConsumer<TConsumer>(string messageName = DefaultHeader.KeyDefaultMessageName)
            where TConsumer : IConsumerMessage
        {
            var consumerKey = GetConsumerKey(GroupId, messageName);
            var consumerType = typeof(TConsumer);
            Binds.AddType(consumerKey, consumerType);
            _services.AddScoped(consumerType);

            var consumerLoggerKey = GetConsumerLoggerKey(messageName);
            var typeConsumerLogger = typeof(ILogger<TConsumer>);
            Binds.AddType(consumerLoggerKey, typeConsumerLogger);

            return this;
        }

        public ListenerConfiguration AddIgnoreEvents(params string[] eventsName)
        {
            _ignoreEvents.AddRange(eventsName);
            return this;
        }

        internal List<string> GetIgnoreEvents => _ignoreEvents;

        internal string GetConsumerKey(string eventName)
          => GetConsumerKey(_groupId, eventName);

        
        internal string GetEventNameFromConsumerKey(string keyConsumer)
            => keyConsumer.Replace(_groupId, "");

        internal void SetSourceTopicName(string sourceTopicName)
            => _sourceTopicName = sourceTopicName;

        internal string GetConsumerLoggerKey(string eventName)
            => $"{GetConsumerKey(_groupId, eventName)}_logger";


        internal static string GetConsumerKey(string groupId, string eventName)
        {
            var sufixName = eventName;

            if (string.IsNullOrWhiteSpace(sufixName))
                sufixName = DefaultHeader.KeyDefaultMessageName;

            return $"{groupId}#{sufixName}";
        }
    }
}