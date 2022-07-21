using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

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

                        try
                        {
                            var msgBody = result.Message.Value;

                            var consumer = scope.ServiceProvider.GetService(_consumerConfiguration.TypeConsumer);

                            var methodGetTypeMessage = _consumerConfiguration.TypeConsumer.GetMethod("GetTypeMessage");
                            var typeMessage = (Type)methodGetTypeMessage.Invoke(consumer, null);
                            
                            var msgParsed = typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultConfiguration.JsonSettings);

                            var methodBeforeConsume = _consumerConfiguration.TypeConsumer.GetMethod("BeforeConsume");
                            methodBeforeConsume.Invoke(consumer, new[] { msgParsed });

                            var methodConsume = _consumerConfiguration.TypeConsumer.GetMethod("Consume");
                            methodConsume.Invoke(consumer, new[] { msgParsed });

                            var methodAfterConsume = _consumerConfiguration.TypeConsumer.GetMethod("AfterConsume");
                            methodAfterConsume.Invoke(consumer, new[] { msgParsed });
                        }
                        catch(Exception ex)
                        {
                            var consumer = scope.ServiceProvider.GetService(_consumerConfiguration.TypeConsumer);
                            var methodErrorConsume = _consumerConfiguration.TypeConsumer.GetMethod("ErrorConsume");
                            methodErrorConsume.Invoke(consumer, new[] { ex });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer?.Unsubscribe();
            _consumer?.Close();
        }
    }
}
