using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Linq;
using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka.BackgroundServices
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
                AutoCommitIntervalMs = 0,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(conf).Build();
            _consumer.Subscribe(_consumerConfiguration.ListenerConfiguration.Topics);
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
                        var consumer = consumerType == null ? null : scope.ServiceProvider.GetService(consumerType);
                        var context = Context.Create(header);

                        try
                        {
                            if (consumer is null)
                            {
                                _consumer.Commit();

                                var skippedConsumer = scope.ServiceProvider.GetService<ISkippedMessage>();
                                if (skippedConsumer != null)
                                {
                                    await skippedConsumer.AlertAsync(context, msgBody);
                                }

                                var skipTopicName = GetTopicNameSkipped(_consumerConfiguration.ListenerConfiguration.GroupId, _consumerConfiguration.ListenerConfiguration.Topics.First());
                                await _producerMessage.ProduceAsync(skipTopicName, new { Message = msgBody }, header, stoppingToken);
                            }
                            else
                            {
                                var sleep = header.GetRetryAt();
                                if (sleep > 0)
                                {
                                    var partitions = _consumer.Assignment;
                                    _consumer.Pause(partitions);
                                    await Task.Delay(sleep);
                                    _consumer.Resume(partitions);
                                }

                                var methodGetTypeMessage = consumerType.GetMethod("GetTypeMessage");
                                var typeMessage = (Type)methodGetTypeMessage.Invoke(consumer, null);
                                var msgParsed = typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultSerializerSettings.JsonSettings);

                                var methodBeforeConsume = consumerType.GetMethod("BeforeConsume");
                                methodBeforeConsume.Invoke(consumer, new[] { context, msgParsed });

                                try
                                {
                                    var methodConsume = consumerType.GetMethod("ConsumeAsync");
                                    var methodConsumeResult = (Task)methodConsume.Invoke(consumer, new[] { context, msgParsed });
                                    methodConsumeResult.Wait();
                                }
                                catch(Exception ex)
                                {
                                    if (_consumerConfiguration.ListenerConfiguration.RetryConfiguration?.First != null && header.GetCurrentAttempt() == 0)
                                    {
                                        var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                            _consumerConfiguration.ListenerConfiguration.Topics.First(),
                                            _consumerConfiguration.ListenerConfiguration.GroupId,
                                            _consumerConfiguration.ListenerConfiguration.RetryConfiguration.First.Minute);

                                        header.AddRetryAt(_consumerConfiguration.ListenerConfiguration.RetryConfiguration.First.Minute, 1);
                                        header.AddWillRetry(true);
                                        await _producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    }
                                    else if (_consumerConfiguration.ListenerConfiguration.RetryConfiguration?.Second != null && header.GetCurrentAttempt() == 1)
                                    {
                                        var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                            _consumerConfiguration.ListenerConfiguration.Topics.First(),
                                            _consumerConfiguration.ListenerConfiguration.GroupId,
                                            _consumerConfiguration.ListenerConfiguration.RetryConfiguration.Second.Minute);

                                        header.AddRetryAt(_consumerConfiguration.ListenerConfiguration.RetryConfiguration.Second.Minute, 2);
                                        header.AddWillRetry(true);
                                        await _producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    }
                                    else if (_consumerConfiguration.ListenerConfiguration.RetryConfiguration?.Third != null && header.GetCurrentAttempt() == 2)
                                    {
                                        var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                            _consumerConfiguration.ListenerConfiguration.Topics.First(),
                                            _consumerConfiguration.ListenerConfiguration.GroupId,
                                            _consumerConfiguration.ListenerConfiguration.RetryConfiguration.Third.Minute);

                                        header.AddRetryAt(_consumerConfiguration.ListenerConfiguration.RetryConfiguration.Third.Minute, 3);
                                        header.AddWillRetry(true);
                                        await _producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    }
                                    else
                                    {
                                        header.AddWillRetry(false);
                                    }

                                    throw ex;
                                }

                                var methodAfterConsume = consumerType.GetMethod("AfterConsume");
                                methodAfterConsume.Invoke(consumer, new[] { context, msgParsed });

                                _consumer.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            _consumer.Commit();
                            if (header.GetWillRetry() is false)
                            {
                                var dlqTopicName = GetTopicNameDeadLetter(_consumerConfiguration.ListenerConfiguration.GroupId, _consumerConfiguration.ListenerConfiguration.Topics.First());
                                await _producerMessage.ProduceAsync(dlqTopicName, new { MessageJson = msgBody, Error = ex }, header, stoppingToken);
                            }
                            var methodErrorConsume = consumerType.GetMethod("ErrorConsume");
                            methodErrorConsume.Invoke(consumer, new[] { context as object, ex });
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

            foreach (var kv in headers)
            {
                var value = Encoding.Default.GetString(kv.GetValueBytes());
                headerValue.PutKeyValue(kv.Key, value);
            }

            return headerValue;
        }

        private string GetTopicNameSkipped(string groupId, string currentTopicName)
        {
            return $"skipped.{groupId}.{currentTopicName}";
        }

        private string GetTopicNameDeadLetter(string groupId, string currentTopicName)
        {
            return $"dlq.{groupId}.{currentTopicName}";
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
