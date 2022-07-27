using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        public BackgroundConsumerManager(IServiceProvider provider, IRegistryListenerService registryListenerService)
        {
            _provider = provider;
            _registryListenerService = registryListenerService;
            _tasks = new List<Task>();
            _producerMessage = (IProducerMessage)_provider.GetService(typeof(IProducerMessage));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var listeners = _registryListenerService.GetListeners();
            foreach (var kv in listeners)
            {
                var task = KafkaConsumer.ExecuteAsync(_provider, kv.Value, _producerMessage, kv.Key, default);
                _tasks.Add(task);
            }

            return base.StartAsync(cancellationToken);
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
