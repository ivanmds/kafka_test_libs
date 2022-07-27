using System;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Notifications;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumer
    {
        private readonly IServiceProvider _provider;
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly IProducerMessage _producerMessage;
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger _logger;

        internal KafkaConsumer(IServiceProvider provider, ListenerConfiguration listenerConfiguration, IProducerMessage producerMessage, ILogger logger)
        {
            _provider = provider;
            _listenerConfiguration = listenerConfiguration;
            _producerMessage = producerMessage;

            var config = KafkaConsumerHelper.GetConsumerConfig(_listenerConfiguration);
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(_listenerConfiguration.TopicName);
            _logger = logger;
        }

        public async Task ExecuteAsync(string processId, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _provider.CreateScope();
                _logger.LogInformation($"Consumer from processId {processId} was started");
                await Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);

                        if (result is null)
                            continue;

                        var msgBody = result.Message.Value;
                        var header = KafkaConsumerHelper.ParseHeader(result.Message.Headers);

                        var consumerKey = _listenerConfiguration.GetConsumerKey(GetEventName(header, msgBody));
                        var consumerType = RegistryTypes.Recover(consumerKey);
                        var consumerClient = consumerType == null ? null : scope.ServiceProvider.GetService(consumerType);
                        var context = ConsumeContext.Create(header);
                        var willRetry = false;

                        try
                        {
                            if (consumerClient is null)
                                await MessageSkippedAsync(scope, context, msgBody, header, stoppingToken);
                            else
                            {
                                var delay = header.GetRetryAt();
                                if (delay > 0)
                                    await Task.Delay(delay);

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
                                    willRetry = await WillRetryAsync(msgParsed, header, stoppingToken);
                                    throw ex;
                                }

                                AfterConsume(consumerType, consumerClient, context, msgParsed);
                                _consumer.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            if (willRetry is false)
                                header.AddWillRetry(false);

                            await MessageErrorAsync(consumerType, consumerClient, context, msgBody, header, ex, stoppingToken);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Consumer from processId {processId} died");
                _consumer.Dispose();
                throw ex;
            }
        }

        private string GetEventName(HeaderValue header, string msgBody)
        {
            try
            {
                if (header.IsNotification())
                    return header.GetEventName();
                else
                {
                    var msgParsed = ParseMessage(typeof(DefaultNotification), msgBody) as DefaultNotification;
                    if (msgParsed is null)
                        return header.GetEventName();

                    return msgParsed.Name;
                }
            }
            catch
            {
                return header.GetEventName();
            }
        }

        private async Task<bool> WillRetryAsync(object msgParsed, HeaderValue header, CancellationToken stoppingToken)
        {
            var retryConfig = _listenerConfiguration.RetryConfiguration;
            var currentAttempt = header.GetCurrentAttempt();
            var nextAttempt = currentAttempt + 1;
            var result = false;

            if (retryConfig != null)
            {
                var retry = retryConfig.GetRetryTimeByAttempt(nextAttempt);
                if (retry != null)
                {
                    var retryTopicName = TopicNameBuilder.GetRetryTopicName(
                                                               _listenerConfiguration.SourceTopicName,
                                                               _listenerConfiguration.GroupId,
                                                               retry.Seconds);

                    header.AddRetryAt(retry.Seconds, nextAttempt);
                    header.AddWillRetry(true);
                    await _producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);

                    result = true;
                }
            }

            return result;
        }

        private Type GetTypeMessage(Type consumerType, object consumerClient)
        {
            var methodGetTypeMessage = consumerType.GetMethod("GetTypeMessage");
            return (Type)methodGetTypeMessage.Invoke(consumerClient, null);
        }

        private void BeforeConsume(Type consumerType, object consumerClient, ConsumeContext context, object msgParsed)
        {
            var methodBeforeConsume = consumerType.GetMethod("BeforeConsume");
            methodBeforeConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private Task ConsumeAsync(Type consumerType, object consumerClient, ConsumeContext context, object msgParsed)
        {
            var methodConsume = consumerType.GetMethod("ConsumeAsync");
            return (Task)methodConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private void AfterConsume(Type consumerType, object consumerClient, ConsumeContext context, object msgParsed)
        {
            var methodAfterConsume = consumerType.GetMethod("AfterConsume");
            methodAfterConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private object ParseMessage(Type typeMessage, string msgBody)
             => typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultSerializerSettings.JsonSettings);

        private async Task MessageSkippedAsync(IServiceScope scope, ConsumeContext context, string msgBody, HeaderValue header, CancellationToken stoppingToken)
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

        private async Task MessageErrorAsync(Type consumerType, object consumerClient, ConsumeContext context, string msgBody, HeaderValue header, Exception ex, CancellationToken stoppingToken)
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
