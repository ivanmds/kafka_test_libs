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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumer : IDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly IProducerMessage _producerMessage;
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger _logger;
        private bool _disposedValue;
        private bool _willStop = false;
        private bool _isProcessing = false;

        internal KafkaConsumer(
            IServiceProvider provider,
            ListenerConfiguration listenerConfiguration,
            IProducerMessage producerMessage,
            ILogger logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _provider = provider;
            _listenerConfiguration = listenerConfiguration;
            _producerMessage = producerMessage;

            var config = KafkaConsumerHelper.GetConsumerConfig(_listenerConfiguration);
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(_listenerConfiguration.TopicName);
            _logger = logger;

            hostApplicationLifetime.ApplicationStopping.Register(async () =>
            {
                _logger.LogWarning($"Consumer from topic {_listenerConfiguration.TopicName} will shutdown");
                _willStop = true;

                while (_isProcessing)
                {
                    await Task.Delay(250);
                }

                _consumer.Unassign();
            });

            hostApplicationLifetime.ApplicationStopped.Register(() => {
                _logger.LogWarning($"Consumer from topic {_listenerConfiguration.TopicName} shutdown");
            });
        }

        public async Task ExecuteAsync(string processId, CancellationToken stoppingToken)
        {
            using var scope = _provider.CreateScope();
            try
            {
                _logger.LogInformation($"Consumer from processId {processId} was started. Process listening topic {_listenerConfiguration.TopicName}.");
                await Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested && !_willStop)
                    {
                        var consume = _consumer.Consume(stoppingToken);

                        if (consume is null)
                            continue;

                        _isProcessing = true;
                        var msgBody = consume.Message.Value;
                        var header = KafkaConsumerHelper.ParseHeader(consume.Message.Headers);

                        var consumerType = GetConsumerType(header, msgBody);
                        var consumerClient = consumerType == null ? null : scope.ServiceProvider.GetService(consumerType);
                        var willRetry = false;

                        try
                        {
                            if (consumerClient is null)
                            {
                                _consumer.Commit();
                                await MessageSkippedAsync(scope, msgBody, header, stoppingToken);
                            }
                            else
                            {
                                var delay = header.GetRetryAt();
                                if (delay > 0)
                                    await Task.Delay(delay);

                                var typeMessage = GetTypeMessage(consumerType, consumerClient);
                                var msgParsed = ParseMessage(typeMessage, msgBody);

                                BeforeConsume(consumerType, consumerClient, header, msgParsed);

                                try
                                {
                                    var methodConsumeResult = ConsumeAsync(consumerType, consumerClient, header, msgParsed);
                                    methodConsumeResult.Wait();
                                }
                                catch (Exception ex)
                                {
                                    willRetry = await WillRetryAsync(msgParsed, header, stoppingToken);
                                    throw ex;
                                }

                                AfterConsume(consumerType, consumerClient, header, msgParsed);
                                _consumer.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            bool shouldBeNotIgnored = ex.Message.Contains("No offset stored") is false;
                            if (shouldBeNotIgnored)
                            {
                                header.AddWillRetry(willRetry);

                                _consumer.Commit();
                                await MessageErrorAsync(consumerType, consumerClient, msgBody, header, ex, stoppingToken);
                            }
                            else
                            {
                                _logger.LogWarning(ex, ex.Message);
                            }
                        }
                        _isProcessing = false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Consumer from processId {processId} died, topic {_listenerConfiguration.TopicName} without consumer.");
                _consumer.Unassign();
                _consumer.Dispose();
                ConsumerErrorFatal(scope, ex);
                throw ex;
            }
        }

        private Type GetConsumerType(HeaderValue header, string msgBody)
        {
            var consumerKey = _listenerConfiguration.GetConsumerKey(GetEventName(header, msgBody));
            var consumerType = Binds.GetType(consumerKey);

            if(consumerType == null)
            {
                consumerKey = _listenerConfiguration.GetConsumerKey(string.Empty);
                consumerType = Binds.GetType(consumerKey);
            }

            return consumerType;
        }

        private string GetEventName(HeaderValue header, string msgBody)
        {
            try
            {
                if (header.GetIsNewClient())
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
                    var retryTopicName = BuilderName.GetTopicNameRetry(
                                                               _listenerConfiguration.SourceTopicName,
                                                               _listenerConfiguration.GroupId,
                                                               retry.Seconds);


                    var headerRetry = HeaderValue.Create();
                    foreach (var kv in header.GetKeyValues())
                        headerRetry.PutKeyValue(kv);

                    headerRetry.AddRetryAt(retry.Seconds, nextAttempt);

                    await _producerMessage.ProduceAsync(retryTopicName, msgParsed, headerRetry, stoppingToken);

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

        private void BeforeConsume(Type consumerType, object consumerClient, HeaderValue header, object msgParsed)
        {
            var context = ConsumeContext.Create(header);

            var methodBeforeConsume = consumerType.GetMethod("BeforeConsume");
            methodBeforeConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private Task ConsumeAsync(Type consumerType, object consumerClient, HeaderValue header, object msgParsed)
        {
            var context = ConsumeContext.Create(header);

            var methodConsume = consumerType.GetMethod("ConsumeAsync");
            return (Task)methodConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private void AfterConsume(Type consumerType, object consumerClient, HeaderValue header, object msgParsed)
        {
            var context = ConsumeContext.Create(header);

            var methodAfterConsume = consumerType.GetMethod("AfterConsume");
            methodAfterConsume.Invoke(consumerClient, new[] { context, msgParsed });
        }

        private object ParseMessage(Type typeMessage, string msgBody)
             => typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultSerializerSettings.JsonSettings);


        private void ConsumerErrorFatal(IServiceScope scope, Exception ex)
        {
            var errorFatal = scope.ServiceProvider.GetService<IConsumerErrorFatal>();
            if (errorFatal != null)
            {
                errorFatal.AlertError(ex);
            }
        }

        private async Task MessageSkippedAsync(IServiceScope scope, string msgBody, HeaderValue header, CancellationToken stoppingToken)
        {
            var context = ConsumeContext.Create(header);
            var skippedConsumer = scope.ServiceProvider.GetService<ISkippedMessage>();
            if (skippedConsumer != null)
            {
                await skippedConsumer.AlertAsync(context, msgBody);
            }

            var skipTopicName = BuilderName.GetTopicNameSkipped(_listenerConfiguration.GroupId, _listenerConfiguration.SourceTopicName);
            await _producerMessage.ProduceAsync(skipTopicName, new { Message = msgBody }, header, stoppingToken);
        }

        private async Task MessageErrorAsync(Type consumerType, object consumerClient, string msgBody, HeaderValue header, Exception ex, CancellationToken stoppingToken)
        {
            var context = ConsumeContext.Create(header);
            if (header.GetWillRetry() is false)
            {
                var dlqTopicName = BuilderName.GetTopicNameDeadLetter(_listenerConfiguration.GroupId, _listenerConfiguration.SourceTopicName);
                await _producerMessage.ProduceAsync(dlqTopicName, new { MessageJson = msgBody, Error = ex }, header, stoppingToken);
            }

            var methodErrorConsume = consumerType.GetMethod("ErrorConsume");
            methodErrorConsume.Invoke(consumerClient, new[] { context as object, ex });
        }

        ~KafkaConsumer() => Dispose(true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _consumer.Close();
                    _consumer.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}