using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Configuration
{
    public static class KafkaConfiguration
    {
        private static IServiceCollection _services;

        public static ConsumerBuilder AddKafka(this IServiceCollection services, KafkaConnection kafkaConnection)
        {
            _services = services;
            var kafkaBuilder = KafkaBuilder.Create(kafkaConnection);

            var kafkaClient = kafkaBuilder.KafkaClient;

            services.AddSingleton(kafkaClient);
            services.AddSingleton((IProducerMessage)kafkaClient);

            var consumerBuilder = new ConsumerBuilder(services, kafkaBuilder);
            return consumerBuilder;
        }
    }
}
