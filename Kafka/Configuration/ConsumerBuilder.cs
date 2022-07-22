using Kafka.BackgroundServices;
using Kafka.Configuration.GroupConsumers;
using Kafka.DefaultValues;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Configuration
{
    public class ConsumerBuilder
    {
        private readonly IServiceCollection _services;
        private readonly KafkaBuilder _kafkaBuilder;
        private ListenerConfiguration _listenerConfiguration;
        private int _numGroupConumer = 0;

        public ConsumerBuilder(IServiceCollection services, KafkaBuilder kafkaBuilder)
        {
            _services = services;
            _kafkaBuilder = kafkaBuilder;
        }

        public ConsumerBuilder CreateListener(string topicName, string groupId)
        {
            _numGroupConumer++;
            _listenerConfiguration = ListenerConfiguration.Create(topicName, groupId, _kafkaBuilder);


            if (_numGroupConumer == 1)
            {
                _services.AddSingleton(GroupConsumerConfigurationOne.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundOne>();
            }

            return this;
        }

        public ConsumerBuilder AddConsumer<TConsumer>(string eventName = DefaultHeader.KeyDefaultEvenName)
            where TConsumer : class
        {
            var consumerKey = GroupConsumerConfigurationBase.GetConsumerKey(_listenerConfiguration.GroupId, eventName);
            var consumerType = typeof(TConsumer);

            //Add validation to add twice the same consumer
            RegistryTypes.Register(consumerKey, consumerType);
            _services.AddSingleton(consumerType);

            return this;
        }

        public KafkaBuilder GetKafkaBuilder => _kafkaBuilder;
    }
}
