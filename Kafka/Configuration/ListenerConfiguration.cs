namespace Bankly.Sdk.Kafka.Configuration
{
    internal class ListenerConfiguration
    {
        private readonly string _topicName;
        private string _sourceTopicName;
        private readonly string _groupId;
        private readonly KafkaBuilder _kafkaBuilder;
        private readonly RetryConfiguration? _retryConfiguration;
        private readonly RetryTime? _retryTime;
        public ListenerConfiguration(string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration, RetryTime? retryTime = null)
        {
            _topicName = topicName;
            _sourceTopicName = topicName;
            _groupId = groupId;
            _kafkaBuilder = kafkaBuilder;
            _retryConfiguration = retryConfiguration;
            _retryTime = retryTime;
        }

        public static ListenerConfiguration Create(string topicName, string groupId, KafkaBuilder kafkaBuilder, RetryConfiguration? retryConfiguration, RetryTime? retryTime = null)
            => new ListenerConfiguration(topicName, groupId, kafkaBuilder, retryConfiguration, retryTime);

        public string TopicName => _topicName;
        public string SourceTopicName => _sourceTopicName;
        public string GroupId => _groupId;
        public KafkaBuilder KafkaBuilder => _kafkaBuilder;
        public RetryConfiguration? RetryConfiguration => _retryConfiguration;
        public RetryTime? RetryTime => _retryTime;


        public string GetConsumerKey(string eventName)
          => GetConsumerKey(_groupId, eventName);

        public string GetEventNameFromConsumerKey(string keyConsumer)
            => keyConsumer.Replace(_groupId, "");

        public void SetSourceTopicName(string sourceTopicName)
            => _sourceTopicName = sourceTopicName;


        public static string GetConsumerKey(string groupId, string eventName)
        {
            var sufixName = eventName;

            if (string.IsNullOrWhiteSpace(sufixName))
                sufixName = DefaultValues.DefaultHeader.KeyDefaultEvenName;

            return $"{groupId}#{sufixName}";
        }
    }
}