using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackground<TConsumer> : BackgroundService
        where TConsumer : class
    {
        private readonly IServiceProvider _services;
        private readonly ConsumerConfiguration<TConsumer> _consumerConfiguration;
        private IConsumer<string, string> _consumer;

        public ConsumerBackground(IServiceProvider services, ConsumerConfiguration<TConsumer> consumerConfiguration)
        {
            _services = services;
            _consumerConfiguration = consumerConfiguration;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = new ConsumerConfig
            {
                GroupId = _consumerConfiguration.ListenerConfiguration.GroupId,
                BootstrapServers = _consumerConfiguration.ListenerConfiguration.KafkaBuilder.KafkaConnection.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, string>(conf).Build();
            _consumer.Subscribe(_consumerConfiguration.ListenerConfiguration.TopicName);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _services.CreateScope();
                await Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);
                        var msgBody = result.Message.Value;

                        var consumer = scope.ServiceProvider.GetService(_consumerConfiguration.TypeConsumer);
                        var methodConsume = _consumerConfiguration.TypeConsumer.GetMethod("Consume");

                        methodConsume.Invoke(consumer, new[] { msgBody });
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
