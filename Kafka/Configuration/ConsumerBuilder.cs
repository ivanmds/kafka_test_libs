using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class ConsumerBuilder
    {
        private readonly IServiceCollection _services;
        private readonly KafkaBuilder _kafkaBuilder;
        private ListenerConfiguration _listenerConfiguration;
        private readonly IRegistryListenerService _registryListenerService;

        public ConsumerBuilder(IServiceCollection services, KafkaBuilder kafkaBuilder)
        {
            _services = services;
            _kafkaBuilder = kafkaBuilder;
            _registryListenerService = new RegistryListenerService();
        }

        public ConsumerBuilder AddSkippedMessage<TSkippedMessage>()
            where TSkippedMessage : ISkippedMessage
        {
            _services.AddSingleton(typeof(ISkippedMessage), typeof(TSkippedMessage));
            return this;
        }

        public ConsumerBuilder CreateListener(string topicName, string groupId, RetryConfiguration? retryConfiguration = null)
        {
            var listenerKey = groupId;
            _listenerConfiguration = ListenerConfiguration.Create(topicName, groupId, _kafkaBuilder, retryConfiguration);
            _registryListenerService.Add(listenerKey, _listenerConfiguration);

            if (retryConfiguration != null)
            {
                foreach (var retry in retryConfiguration.GetRetries())
                {
                    var retryTopicName = ListenerConfiguration.GetRetryTopicName(topicName, groupId, retry.Seconds);
                    
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

        public ConsumerBuilder AddConsumer<TConsumer>(string eventName = DefaultHeader.KeyDefaultEvenName)
            where TConsumer : class
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
