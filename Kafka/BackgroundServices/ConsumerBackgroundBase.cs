﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Configuration.GroupConsumers;
using Kafka.Consumers;
using Kafka.DefaultValues;
using Kafka.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundBase<TGroupConsumerConfiguration> : BackgroundService
        where TGroupConsumerConfiguration : GroupConsumerConfigurationBase
    {
        private readonly IServiceProvider _services;
        private readonly TGroupConsumerConfiguration _consumerConfiguration;
        private readonly IProducerMessage _producerMessage;

        private IConsumer<string, string> _consumer;

        public ConsumerBackgroundBase(IServiceProvider services, TGroupConsumerConfiguration consumerConfiguration, IProducerMessage producerMessage)
        {
            _services = services;
            _consumerConfiguration = consumerConfiguration;
            _producerMessage = producerMessage;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = new ConsumerConfig
            {
                GroupId = _consumerConfiguration.ListenerConfiguration.GroupId,
                BootstrapServers = _consumerConfiguration.ListenerConfiguration.KafkaBuilder.KafkaConnection.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
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
                        var header = ParseHeader(result.Message.Headers);

                        var consumerKey = _consumerConfiguration.GetConsumerKey(header.GetEventName());
                        var consumerType = RegistryTypes.Recover(consumerKey);

                        var consumer = scope.ServiceProvider.GetService(consumerType);

                        try
                        {
                            if(consumer is null)
                            {
                                var methodErrorConsume = consumerType.GetMethod("SkiperConsume");
                                methodErrorConsume.Invoke(consumer, null);
                                _consumer.Commit();

                                var skipTopicName = GetTopicNameSkiped(_consumerConfiguration.ListenerConfiguration.GroupId, _consumerConfiguration.ListenerConfiguration.TopicName);
                                await _producerMessage.ProduceAsync(skipTopicName, "001", msgBody, header, stoppingToken);
                            }

                            var context = Context.Create(header);
                            var methodGetTypeMessage = consumerType.GetMethod("GetTypeMessage");
                            var typeMessage = (Type)methodGetTypeMessage.Invoke(consumer, null);

                            var msgParsed = typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultSerializerSettings.JsonSettings);

                            var methodBeforeConsume = consumerType.GetMethod("BeforeConsume");
                            methodBeforeConsume.Invoke(consumer, new[] { context, msgParsed });

                            var methodConsume = consumerType.GetMethod("ConsumeAsync");
                            var methodConsumeResult = (Task)methodConsume.Invoke(consumer, new[] { context, msgParsed });
                            methodConsumeResult.Wait(); 

                            var methodAfterConsume = consumerType.GetMethod("AfterConsume");
                            methodAfterConsume.Invoke(consumer, new[] { context, msgParsed });

                            _consumer.Commit();
                        }
                        catch (Exception ex)
                        {
                            var methodErrorConsume = consumerType.GetMethod("ErrorConsume");
                            methodErrorConsume.Invoke(consumer, new[] { ex });
                            _consumer.Commit();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private HeaderValue ParseHeader(Headers headers)
        {
            var headerValue = HeaderValue.Create();

            if (headers is null)
                return headerValue;

            foreach(var kv in headers)
            {
                var value = Encoding.Default.GetString(kv.GetValueBytes());
                headerValue.PutKeyValue(kv.Key, value);
            }

            return headerValue;
        }

        private string GetTopicNameSkiped(string groupId, string currentTopicName)
        {
            return $"skip.{groupId}.{currentTopicName}";
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _consumer?.Unsubscribe();
                _consumer?.Close();
            });
        }
    }
}
