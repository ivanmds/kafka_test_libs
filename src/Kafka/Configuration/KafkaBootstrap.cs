using Bankly.Sdk.Kafka.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public static class KafkaBootstrap
    {
        public static KafkaConfiguration AddKafka(this IServiceCollection services, KafkaConnection kafkaConnection)
        {
            var kafkaBuilder = KafkaBuilder.Create(kafkaConnection);

            var kafkaClient = kafkaBuilder.KafkaClient;
            services.AddSingleton(kafkaClient);
            services.AddSingleton((IProducerMessage)kafkaClient);

            var kafkaAdminClient = new KafkaAdminClient(kafkaConnection);
            services.AddSingleton((IKafkaAdminClient)kafkaAdminClient);

            var kafkaConfiguration = new KafkaConfiguration(services, kafkaBuilder);
            return kafkaConfiguration;
        }
    }
}
