using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Kafka.Clients;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Metrics;
using Bankly.Sdk.Kafka.Services;
using Bankly.Sdk.Kafka.Traces;
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
        private readonly IMetricService _metricService;
        private readonly ITraceService _traceService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public BackgroundConsumerManager(
            IServiceProvider provider,
            IRegistryListenerService registryListenerService,
            IKafkaAdminClient kafkaAdminClient,
            ILogger<BackgroundConsumerManager> logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _provider = provider;
            _registryListenerService = registryListenerService;
            _tasks = new List<Task>();
            _producerMessage = (IProducerMessage)_provider.GetService(typeof(IProducerMessage));
            _kafkaAdminClient = kafkaAdminClient;
            _topicNames = new List<string>();
            _logger = logger;
            _metricService = (IMetricService)_provider.GetService(typeof(IMetricService));
            _traceService = (ITraceService)_provider.GetService(typeof(ITraceService));
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var listeners = _registryListenerService.GetListeners().ToArray();

            foreach(var kv in listeners)
            {
                var listener = kv.Value;
                _topicNames.Add(listener.TopicName);

                var task = CreateConsumerProcess(kv.Key, listener, cancellationToken);
                _tasks.Add(task);
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(_topicNames.Select(topicName => _kafkaAdminClient.PutTopicAsync(topicName)));
            await Task.WhenAll(_tasks.ToArray());
        }

        private async Task ConsumerContinueWith(Task continueTask)
        {
            if(continueTask.IsFaulted is false)
                return;
            var consumerId = $"task_id_{continueTask.Id}";
            var listener = _registryListenerService.Get(consumerId);
            if(listener != null)
            {
                await CreateConsumerProcess(consumerId, listener, default);
            }
        }

        private Task CreateConsumerProcess(string processId, ListenerConfiguration listener, CancellationToken cancellationToken)
        {
            IKafkaConsumer kafkaConsumer;

            if(listener.useAvro)
                kafkaConsumer = new KafkaConsumerAvroGenericRecord(_provider, listener, _producerMessage, _logger, _metricService, _traceService, _hostApplicationLifetime);
            else
                kafkaConsumer = new KafkaConsumerString(_provider, listener, _producerMessage, _logger, _metricService, _traceService, _hostApplicationLifetime);


            var task = kafkaConsumer.ExecuteAsync(processId, cancellationToken);
            task.ContinueWith(ConsumerContinueWith);

            _registryListenerService.Add($"task_id_{task.Id}", listener);

            return task;
        }

    }
}
