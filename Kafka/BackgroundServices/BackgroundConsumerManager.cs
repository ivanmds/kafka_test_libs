using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Kafka.Clients;
using Bankly.Sdk.Kafka.Services;
using Microsoft.Extensions.Hosting;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class BackgroundConsumerManager : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IRegistryListenerService _registryListenerService;
        private readonly List<Task> _tasks;
        private readonly IProducerMessage _producerMessage;
        private readonly IKafkaAdminClient _kafkaAdminClient;

        public BackgroundConsumerManager(IServiceProvider provider, IRegistryListenerService registryListenerService, IKafkaAdminClient kafkaAdminClient)
        {
            _provider = provider;
            _registryListenerService = registryListenerService;
            _tasks = new List<Task>();
            _producerMessage = (IProducerMessage)_provider.GetService(typeof(IProducerMessage));
            _kafkaAdminClient = kafkaAdminClient;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var listeners = _registryListenerService.GetListeners();

            foreach (var kv in listeners)
            {
                var listener = kv.Value;
                if (listener.RetryTime != null)
                    CheckTopic(listener.TopicName);

                var kafkaConsumer = new KafkaConsumer(_provider, listener, _producerMessage);
                var task = kafkaConsumer.ExecuteAsync(default);
                _tasks.Add(task);
            }

            return base.StartAsync(cancellationToken);
        }

        private void CheckTopic(string topicName)
        {
            _kafkaAdminClient.PutTopicAsync(topicName).Wait();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Task.WaitAll(_tasks.ToArray());
            });
        }
    }
}
