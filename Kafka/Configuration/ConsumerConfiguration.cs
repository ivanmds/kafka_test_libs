using Bankly.Sdk.Kafka.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class ConsumerConfiguration
    {
        private readonly KafkaConfiguration _kafkaConfiguration;
        private readonly IServiceCollection _services;
        private readonly KafkaBuilder _kafkaBuilder;
        private ListenerConfiguration _listenerConfiguration;
        private readonly IRegistryListenerService _registryListenerService;

        public ConsumerConfiguration(KafkaConfiguration kafkaConfiguration)
        {
            _kafkaConfiguration = kafkaConfiguration;
            _services = _kafkaConfiguration.GetServiceCollection();
            _kafkaBuilder = kafkaConfiguration.GetKafkaBuilder();
            _registryListenerService = new RegistryListenerService();
        }

        public ListenerConfiguration CreateListener(string topicName, string groupId, RetryConfiguration? retryConfiguration = null)
        {
            var listenerKey = groupId;
            _listenerConfiguration = ListenerConfiguration.Create(_services, topicName, groupId, _kafkaBuilder, retryConfiguration);
            _registryListenerService.Add(listenerKey, _listenerConfiguration);

            if (retryConfiguration != null)
            {
                foreach (var retry in retryConfiguration.GetRetries())
                {
                    var retryTopicName = TopicNameBuilder.GetRetryTopicName(topicName, groupId, retry.Seconds);

                    var retryListenerConfiguration = ListenerConfiguration.Create(_services, retryTopicName, groupId, _kafkaBuilder, retryConfiguration, retry);
                    retryListenerConfiguration.SetSourceTopicName(topicName);

                    listenerKey = $"retry_{retry.Seconds}s_{groupId}";
                    _registryListenerService.Add(listenerKey, retryListenerConfiguration);
                }
            }

            _services.AddSingleton(_registryListenerService);
            _services.AddHostedService<BackgroundServices.BackgroundConsumerManager>();
            return _listenerConfiguration;
        }
    }
}
