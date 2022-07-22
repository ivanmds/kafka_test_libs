using Bankly.Sdk.Kafka.Clients;
using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka
{
    public class KafkaBuilder
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly KafkaConnection _kafkaConnection;

        public KafkaBuilder(KafkaConnection kafkaConnection)
        {
            _kafkaConnection = kafkaConnection;
            _kafkaClient = new KafkaClient(kafkaConnection);
        }

        public static KafkaBuilder Create(KafkaConnection kafkaConnection)
            => new KafkaBuilder(kafkaConnection);

        public IKafkaClient KafkaClient => _kafkaClient;
        public KafkaConnection KafkaConnection => _kafkaConnection;
    }
}
