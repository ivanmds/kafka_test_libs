using Bankly.Sdk.Kafka.BackgroundServices;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
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

        public ConsumerBuilder AddSkippedMessage<TSkippedMessage>()
            where TSkippedMessage : ISkippedMessage
        {
            _services.AddSingleton(typeof(ISkippedMessage), typeof(TSkippedMessage));
            return this;
        }

        public ConsumerBuilder CreateListener(string topicName, string groupId, RetryConfiguration? retryConfiguration = null)
        {
            _numGroupConumer++;
            _listenerConfiguration = ListenerConfiguration.Create(topicName, groupId, _kafkaBuilder, retryConfiguration);

            if (_numGroupConumer == 1)
            {
                _services.AddSingleton(GroupConsumerConfigurationOne.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundOne>();
            }
            else if (_numGroupConumer == 2)
            {
                _services.AddSingleton(GroupConsumerConfigurationTwo.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundTwo>();
            }
            else if (_numGroupConumer == 3)
            {
                _services.AddSingleton(GroupConsumerConfigurationThree.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundThree>();
            }
            else if (_numGroupConumer == 4)
            {
                _services.AddSingleton(GroupConsumerConfigurationFour.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundFour>();
            }
            else if (_numGroupConumer == 5)
            {
                _services.AddSingleton(GroupConsumerConfigurationFive.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundFive>();
            }
            else if (_numGroupConumer == 6)
            {
                _services.AddSingleton(GroupConsumerConfigurationSix.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundSix>();
            }
            else if (_numGroupConumer == 7)
            {
                _services.AddSingleton(GroupConsumerConfigurationSeven.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundSeven>();
            }
            else if (_numGroupConumer == 8)
            {
                _services.AddSingleton(GroupConsumerConfigurationEight.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundEight>();
            }
            else if (_numGroupConumer == 9)
            {
                _services.AddSingleton(GroupConsumerConfigurationNine.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundNine>();
            }
            else if (_numGroupConumer == 10)
            {
                _services.AddSingleton(GroupConsumerConfigurationTen.Create(_listenerConfiguration));
                _services.AddHostedService<ConsumerBackgroundTen>();
            }
            else
            {
                throw new System.Exception("Total groupId maximum is ten");
            }

            return this;
        }

        public ConsumerBuilder AddConsumer<TConsumer>(string eventName = DefaultHeader.KeyDefaultEvenName)
            where TConsumer : class
        {
            var consumerKey = GroupConsumerConfigurationBase.GetConsumerKey(_listenerConfiguration.GroupId, eventName);
            var consumerType = typeof(TConsumer);

            //Add validation when add twice the same consumer
            RegistryTypes.Register(consumerKey, consumerType);
            _services.AddSingleton(consumerType);

            return this;
        }

        public KafkaBuilder GetKafkaBuilder => _kafkaBuilder;
    }
}
