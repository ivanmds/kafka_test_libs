using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Kafka.Clients;
using Bankly.Sdk.Kafka.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class BackgroundConsumerManager : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IRegistryListenerService _registryListenerService;
        private readonly List<Task> _tasks;
        private readonly List<string> _topicNames;
        private readonly IProducerMessage _producerMessage;
        private readonly IKafkaAdminClient _kafkaAdminClient;
        private readonly ILogger<BackgroundConsumerManager> _logger;

        public BackgroundConsumerManager(IServiceProvider provider, IRegistryListenerService registryListenerService, IKafkaAdminClient kafkaAdminClient, ILogger<BackgroundConsumerManager> logger)
        {
            _provider = provider;
            _registryListenerService = registryListenerService;
            _tasks = new List<Task>();
            _producerMessage = (IProducerMessage)_provider.GetService(typeof(IProducerMessage));
            _kafkaAdminClient = kafkaAdminClient;
            _topicNames = new List<string>();
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var listeners = _registryListenerService.GetListeners().ToArray();

            foreach (var kv in listeners)
            {
                var listener = kv.Value;
                if (listener.RetryTime != null)
                    _topicNames.Add(listener.TopicName);

                var kafkaConsumer = new KafkaConsumer(_provider, listener, _producerMessage, _logger);
                var task = kafkaConsumer.ExecuteAsync(kv.Key, cancellationToken);
                task.ContinueWith(ConsumerContinueWith);

                _registryListenerService.Add($"task_id_{task.Id}", listener);
                _tasks.Add(task);
            }

            await base.StartAsync(cancellationToken);
        }

        private async Task ConsumerContinueWith(Task continueTask)
        {
            if (continueTask.IsFaulted is false)
                return;
            var consumerId = $"task_id_{continueTask.Id}";
            var listener = _registryListenerService.Get(consumerId);
            if(listener != null)
            {
                var kafkaConsumer = new KafkaConsumer(_provider, listener, _producerMessage, _logger);
                var task = kafkaConsumer.ExecuteAsync(consumerId, default);
                task.ContinueWith(ConsumerContinueWith);

                _registryListenerService.Add($"task_id_{task.Id}", listener);
                await Task.Run(() =>
                {
                    task.Wait();
                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => {
                var taskPutTopic = _topicNames.Select(topicName => _kafkaAdminClient.PutTopicAsync(topicName));
                Task.WaitAll(taskPutTopic.ToArray());
            });

            await Task.Run(() =>
            {
                Task.WaitAll(_tasks.ToArray());
            });
        }
    }
}
