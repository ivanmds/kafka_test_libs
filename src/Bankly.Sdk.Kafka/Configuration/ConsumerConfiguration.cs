using System.Collections.Generic;
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

        public ListenerConfiguration CreateListeners(List<string> topicNames, string groupId, RetryConfiguration? retryConfiguration = null, bool useAvro = false)
        {
            foreach (var topicName in topicNames)
                CreateListener(topicName, groupId, retryConfiguration, useAvro);

            return _listenerConfiguration;
        }

        public ListenerConfiguration CreateListener(string topicName, string groupId, RetryConfiguration? retryConfiguration = null, bool useAvro = false)
        {
            var listenerKey = $"{groupId}-{topicName}";
            _listenerConfiguration = ListenerConfiguration.Create(_services, topicName, groupId, _kafkaBuilder, retryConfiguration, useAvro:useAvro);
            _registryListenerService.Add(listenerKey, _listenerConfiguration);

            if (retryConfiguration != null)
            {
                foreach (var retry in retryConfiguration.GetRetries())
                {
                    var retryTopicName = BuilderName.GetTopicNameRetry(topicName, groupId, retry.Seconds);

                    var retryListenerConfiguration = ListenerConfiguration.Create(_services, retryTopicName, groupId, _kafkaBuilder, retryConfiguration, retry, useAvro);
                    retryListenerConfiguration.SetSourceTopicName(topicName);

                    listenerKey = $"retry_{retry.Seconds}s_{listenerKey}";
                    _registryListenerService.Add(listenerKey, retryListenerConfiguration);
                }
            }

            _services.AddSingleton(_registryListenerService);
            _services.AddHostedService<BackgroundServices.BackgroundConsumerManager>();
            return _listenerConfiguration;
        }
    }
}
