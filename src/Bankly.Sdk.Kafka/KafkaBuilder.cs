using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka
{
    public class KafkaBuilder
    {
        private readonly KafkaConnection _kafkaConnection;

        public KafkaBuilder(KafkaConnection kafkaConnection)
        {
            _kafkaConnection = kafkaConnection;
        }

        public static KafkaBuilder Create(KafkaConnection kafkaConnection)
            => new KafkaBuilder(kafkaConnection);

        public KafkaConnection KafkaConnection => _kafkaConnection;
    }
}
