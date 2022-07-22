namespace Bankly.Sdk.Kafka.Configuration
{
    public class ListenerConfiguration
    {
        private readonly string _topicName;
        private readonly string _groupId;
        private readonly KafkaBuilder _kafkaBuilder;

        public ListenerConfiguration(string topicName, string groupId, KafkaBuilder kafkaBuilder)
        {
            _topicName = topicName;
            _groupId = groupId;
            _kafkaBuilder = kafkaBuilder;
        }

        public static ListenerConfiguration Create(string topicName, string groupId, KafkaBuilder kafkaBuilder)
            => new ListenerConfiguration(topicName, groupId, kafkaBuilder);

        public string TopicName => _topicName;
        public string GroupId => _groupId;
        public KafkaBuilder KafkaBuilder => _kafkaBuilder;
    }
}
