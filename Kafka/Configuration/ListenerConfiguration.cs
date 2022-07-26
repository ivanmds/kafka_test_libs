using System.Collections.Generic;
using System.Linq;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class ListenerConfiguration
    {
        private readonly List<string> _topics;
        private readonly string _groupId;
        private readonly KafkaBuilder _kafkaBuilder;
        private readonly RetryConfiguration? _retryConfiguration;
        public ListenerConfiguration(string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration)
        {
            _topics = new List<string>
            {
                topicName
            };

            _groupId = groupId;
            _kafkaBuilder = kafkaBuilder;
            _retryConfiguration = retryConfiguration;
            SetRetryTopicName();
        }

        public static ListenerConfiguration Create(string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration)
            => new ListenerConfiguration(topicName, groupId, kafkaBuilder, retryConfiguration);

        public IReadOnlyCollection<string> Topics => _topics;
        public string GroupId => _groupId;
        public KafkaBuilder KafkaBuilder => _kafkaBuilder;
        public RetryConfiguration? RetryConfiguration => _retryConfiguration;

        private void SetRetryTopicName()
        {
            if (_retryConfiguration?.First != null)
            {
                var topicName = GetRetryTopicName(_topics.First(), _groupId, _retryConfiguration.First.Minute);
                _topics.Add(topicName);
            }
            
            if (_retryConfiguration?.Second != null)
            {
                var topicName = GetRetryTopicName(_topics.First(), _groupId, _retryConfiguration.Second.Minute);
                _topics.Add(topicName);
            }

            if (_retryConfiguration?.Third != null)
            {
                var topicName = GetRetryTopicName(_topics.First(), _groupId, _retryConfiguration.Third.Minute);
                _topics.Add(topicName);
            }
        }

        public static string GetRetryTopicName(string currentTopic, string groupId, int timeMinutes)
            => $"retry_{timeMinutes}.{groupId}.{currentTopic}";
    }
}