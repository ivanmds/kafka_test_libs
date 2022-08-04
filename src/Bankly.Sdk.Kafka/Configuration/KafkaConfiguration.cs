using Bankly.Sdk.Kafka.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class KafkaConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly KafkaBuilder _kafkaBuilder;

        internal IServiceCollection GetServiceCollection() => _services;
        internal KafkaBuilder GetKafkaBuilder() => _kafkaBuilder;

        public KafkaConfiguration(IServiceCollection services, KafkaBuilder kafkaBuilder)
        {
            _services = services;
            _kafkaBuilder = kafkaBuilder;
        }

        public KafkaConfiguration Bind<TMessage>(string topicName)
            where TMessage : class
        {
            var key = typeof(TMessage).FullName;
            Binds.AddString(key, topicName);

            return this;
        }

        public KafkaConfiguration AddSkippedMessage<TSkippedMessage>()
            where TSkippedMessage : ISkippedMessage
        {
            _services.AddSingleton(typeof(ISkippedMessage), typeof(TSkippedMessage));
            return this;
        }

        public KafkaConfiguration AddConsumerErrorFatal<TErrorFatal>()
            where TErrorFatal : IConsumerErrorFatal
        {
            _services.AddSingleton(typeof(IConsumerErrorFatal), typeof(TErrorFatal));
            return this;
        }

        public ConsumerConfiguration GetConsumerBuilder()
            => new ConsumerConfiguration(this);
    }
}
