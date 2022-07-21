using Kafka.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Configuration
{
    public class ConsumerBuilder
    {
        private readonly IServiceCollection _services;
        private readonly KafkaBuilder _kafkaBuilder;
        private ListenerConfiguration _listernerConfiguration;

        public ConsumerBuilder(IServiceCollection services, KafkaBuilder kafkaBuilder)
        {
            _services = services;
            _kafkaBuilder = kafkaBuilder;
        }


        public ConsumerBuilder CreateListerner(string topicName, string groupId)
        {
            _listernerConfiguration = ListenerConfiguration.Create(topicName, groupId, _kafkaBuilder);
            return this;
        }

        public ConsumerBuilder AddConsumer<TMessage, TConsumer>(string? eventName = null)
            where TMessage : class
            where TConsumer : class
        {
            var consumerConfiguration = ConsumerConfiguration<TConsumer>.Create<TMessage>(_listernerConfiguration, eventName);

            
            _services.AddSingleton(consumerConfiguration.TypeConsumer);
            _services.AddSingleton(consumerConfiguration);
            _services.AddHostedService<ConsumerBackground<TConsumer>>();
            return this;
        }

        public KafkaBuilder GetKafkaBuilder => _kafkaBuilder;
    }
}
