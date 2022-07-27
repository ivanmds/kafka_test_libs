using Bankly.Sdk.Kafka.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public static class KafkaConfiguration
    {
        public static ConsumerBuilder AddKafka(this IServiceCollection services, KafkaConnection kafkaConnection)
        {
            var kafkaBuilder = KafkaBuilder.Create(kafkaConnection);

            var kafkaClient = kafkaBuilder.KafkaClient;
            services.AddSingleton(kafkaClient);
            services.AddSingleton((IProducerMessage)kafkaClient);

            var kafkaAdminClient = new KafkaAdminClient(kafkaConnection);
            services.AddSingleton((IKafkaAdminClient)kafkaAdminClient);

            var consumerBuilder = new ConsumerBuilder(services, kafkaBuilder);
            return consumerBuilder;
        }
    }
}
