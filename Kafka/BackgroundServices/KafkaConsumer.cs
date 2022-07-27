using System;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumer
    {
        private readonly IServiceProvider _provider;
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly IProducerMessage _producerMessage;
        private readonly IConsumer<string, string> _consumer;

        internal KafkaConsumer(IServiceProvider provider, ListenerConfiguration listenerConfiguration, IProducerMessage producerMessage)
        {
            _provider = provider;
            _listenerConfiguration = listenerConfiguration;
            _producerMessage = producerMessage;

            var config = KafkaConsumerHelper.GetConsumerConfig(_listenerConfiguration);
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(_listenerConfiguration.TopicName);
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);

                        if (result is null)
                            continue;

                        var msgBody = result.Message.Value;
                        var header = KafkaConsumerHelper.ParseHeader(result.Message.Headers);

                        var consumerKey = _listenerConfiguration.GetConsumerKey(header.GetEventName());
                        var consumerType = RegistryTypes.Recover(consumerKey);
                        var consumerClient = consumerType == null ? null : scope.ServiceProvider.GetService(consumerType);
                        var context = Context.Create(header);

                        try
                        {
                            if (consumerClient is null)
                                await MessageSkippedAsync(scope, context, msgBody, header, stoppingToken);
                            else
                            {
                                var sleep = header.GetRetryAt();
                                if (sleep > 0)
                                    await Task.Delay(sleep);

                                var typeMessage = GetTypeMessage(consumerType, consumerClient);
                                var msgParsed = ParseMessage(typeMessage, msgBody);

                                BeforeConsume(consumerType, consumerClient, context, msgParsed);

                                try
                                {
                                    var methodConsumeResult = ConsumeAsync(consumerType, consumerClient, context, msgParsed);
                                    methodConsumeResult.Wait();
                                }
                                catch (Exception ex)
                                {
                                    var retryConfig = _listenerConfiguration.RetryConfiguration;

                                    if (retryConfig != null && header.GetCurrentAttempt() == 0)
                                    {
                                        var retry = retryConfig.GetRetryTimeByAttempt(1);
                                        if (retry != null)
                                        {
                                            var retryTopicName = TopicNameBuilder.GetRetryTopicName(
                                                                                       _listenerConfiguration.SourceTopicName,
                                                                                       _listenerConfiguration.GroupId,
                                                                                       retry.Seconds);
                                            header.AddRetryAt(retry.Seconds, 1);
                                            header.AddWillRetry(true);
                                            await _producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                        }
                                    }

                                    //else if (listenerConfiguration.RetryConfiguration?.Second != null && header.GetCurrentAttempt() == 1)
                                    //{
                                    //    var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                    //        listenerConfiguration.SourceTopicName,
                                    //        listenerConfiguration.GroupId,
                                    //        listenerConfiguration.RetryConfiguration.Second.Minute);

                                    //    header.AddRetryAt(listenerConfiguration.RetryConfiguration.Second.Minute, 2);
                                    //    header.AddWillRetry(true);
                                    //    await producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    //}
                                    //else if (listenerConfiguration.RetryConfiguration?.Third != null && header.GetCurrentAttempt() == 2)
                                    //{
                                    //    var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                    //        listenerConfiguration.SourceTopicName,
                                    //        listenerConfiguration.GroupId,
                                    //        listenerConfiguration.RetryConfiguration.Third.Minute);

                                    //    header.AddRetryAt(listenerConfiguration.RetryConfiguration.Third.Minute, 3);
                                    //    header.AddWillRetry(true);
                                    //    await producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    //}
                                    else
                                    {
                                        header.AddWillRetry(false);
                                    }

                                    throw ex;
                                }

                                AfterConsume(consumerType, consumerClient, context, msgParsed);
                                _consumer.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            await MessageErrorAsync(consumerType, consumerClient, context, msgBody, header, ex, stoppingToken);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _consumer.Dispose();
                throw ex;
            }
        }

        private Type GetTypeMessage(Type consumerType, object consumerClient)
        {
            var methodGetTypeMessage = consumerType.GetMethod("GetTypeMessage");
            return (Type)methodGetTypeMessage.Invoke(consumerClient, null);
        }

        private void BeforeConsume(Type consumerType, object consumerClient, Context context, object msgParsed)
        {
            var methodBeforeConsume = consumerType.GetMethod("BeforeConsume");
            methodBeforeConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private Task ConsumeAsync(Type consumerType, object consumerClient, Context context, object msgParsed)
        {
            var methodConsume = consumerType.GetMethod("ConsumeAsync");
            return (Task)methodConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private void AfterConsume(Type consumerType, object consumerClient, Context context, object msgParsed)
        {
            var methodAfterConsume = consumerType.GetMethod("AfterConsume");
            methodAfterConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private object ParseMessage(Type typeMessage, string msgBody)
             => typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultSerializerSettings.JsonSettings);

        private async Task MessageSkippedAsync(IServiceScope scope, Context context, string msgBody, HeaderValue header, CancellationToken stoppingToken)
        {
            _consumer.Commit();

            var skippedConsumer = scope.ServiceProvider.GetService<ISkippedMessage>();
            if (skippedConsumer != null)
            {
                await skippedConsumer.AlertAsync(context, msgBody);
            }

            var skipTopicName = KafkaConsumerHelper.GetTopicNameSkipped(_listenerConfiguration.GroupId, _listenerConfiguration.SourceTopicName);
            await _producerMessage.ProduceAsync(skipTopicName, new { Message = msgBody }, header, stoppingToken);
        }

        private async Task MessageErrorAsync(Type consumerType, object consumerClient, Context context, string msgBody, HeaderValue header, Exception ex, CancellationToken stoppingToken)
        {
            _consumer.Commit();
            if (header.GetWillRetry() is false)
            {
                var dlqTopicName = KafkaConsumerHelper.GetTopicNameDeadLetter(_listenerConfiguration.GroupId, _listenerConfiguration.SourceTopicName);
                await _producerMessage.ProduceAsync(dlqTopicName, new { MessageJson = msgBody, Error = ex }, header, stoppingToken);
            }
            var methodErrorConsume = consumerType.GetMethod("ErrorConsume");
            methodErrorConsume.Invoke(consumerClient, new[] { context as object, ex });
        }
    }
}
