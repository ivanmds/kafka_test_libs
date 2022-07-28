using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class ConsumerConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly KafkaBuilder _kafkaBuilder;
        private ListenerConfiguration _listenerConfiguration;
        private readonly IRegistryListenerService _registryListenerService;

        public ConsumerConfiguration(IServiceCollection services, KafkaBuilder kafkaBuilder)
        {
            _services = services;
            _kafkaBuilder = kafkaBuilder;
            _registryListenerService = new RegistryListenerService();
        }

        public ConsumerConfiguration AddSkippedMessage<TSkippedMessage>()
            where TSkippedMessage : ISkippedMessage
        {
            _services.AddSingleton(typeof(ISkippedMessage), typeof(TSkippedMessage));
            return this;
        }

        public ConsumerConfiguration AddConsumerErrorFatal<TErrorFatal>()
           where TErrorFatal : IConsumerErrorFatal
        {
            _services.AddSingleton(typeof(IConsumerErrorFatal), typeof(TErrorFatal));
            return this;
        }

        public ConsumerConfiguration CreateListener(string topicName, string groupId, RetryConfiguration? retryConfiguration = null)
        {
            var listenerKey = groupId;
            _listenerConfiguration = ListenerConfiguration.Create(topicName, groupId, _kafkaBuilder, retryConfiguration);
            _registryListenerService.Add(listenerKey, _listenerConfiguration);

            if (retryConfiguration != null)
            {
                foreach (var retry in retryConfiguration.GetRetries())
                {
                    var retryTopicName = TopicNameBuilder.GetRetryTopicName(topicName, groupId, retry.Seconds);
                    
                    var retryListenerConfiguration = ListenerConfiguration.Create(retryTopicName, groupId, _kafkaBuilder, retryConfiguration, retry);
                    retryListenerConfiguration.SetSourceTopicName(topicName);
                    
                    listenerKey = $"retry_{retry.Seconds}s_{groupId}";
                    _registryListenerService.Add(listenerKey, retryListenerConfiguration);
                }
            }

            _services.AddSingleton(_registryListenerService);
            _services.AddHostedService<BackgroundServices.BackgroundConsumerManager>();
            return this;
        }

        public ConsumerConfiguration AddConsumer<TConsumer>(string eventName = DefaultHeader.KeyDefaultEvenName)
            where TConsumer : IConsumerMessage
        {
            var consumerKey = ListenerConfiguration.GetConsumerKey(_listenerConfiguration.GroupId, eventName);
            var consumerType = typeof(TConsumer);

            //Add validation when add twice the same consumer
            RegistryTypes.Register(consumerKey, consumerType);
            _services.AddSingleton(consumerType);

            return this;
        }
    }
}
