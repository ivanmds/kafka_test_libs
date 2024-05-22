using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avro.Generic;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Exceptions;
using Bankly.Sdk.Kafka.Avro;
using Bankly.Sdk.Kafka.Metrics;
using Bankly.Sdk.Kafka.Notifications;
using Bankly.Sdk.Kafka.Traces;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal abstract class KafkaConsumer<TMessage> : IDisposable, IKafkaConsumer
    {
        private readonly IServiceProvider _provider;
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly IProducerMessage _producerMessage;
        private readonly IConsumer<string, TMessage> _consumer;
        private readonly ILogger _logger;
        private readonly IMetricService _metricService;
        private readonly ITraceService _traceService;
        private readonly KeyValuePair<string, object?> _tagCunsumerGroupId;
        private readonly Type TypeMessage;
        private bool _disposedValue;
        private bool _willStop = false;
        private bool _isProcessing = false;
        private bool messageTypeIsString = true;

        internal KafkaConsumer(
            IServiceProvider provider,
            ListenerConfiguration listenerConfiguration,
            IProducerMessage producerMessage,
            ILogger logger,
            IMetricService metricService,
            ITraceService traceService,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _provider = provider;
            _listenerConfiguration = listenerConfiguration;
            _producerMessage = producerMessage;
            _metricService = metricService;
            _traceService = traceService;

            var config = KafkaConsumerHelper.GetConsumerConfig(_listenerConfiguration);

            var TypeMessage = typeof(TMessage);
            if (TypeMessage.FullName == typeof(string).FullName)
            {
                _consumer = new ConsumerBuilder<string, TMessage>(config).Build();
            }
            else if (TypeMessage.FullName == typeof(GenericRecord).FullName)
            {
                var schemaRegistryClient = KafkaConsumerHelper.GetCachedSchemaRegistryClient(_listenerConfiguration);

                _consumer = new ConsumerBuilder<string, TMessage>(config)
               .SetValueDeserializer(new AvroDeserializer<TMessage>(schemaRegistryClient).AsSyncOverAsync())
               .SetErrorHandler((_, e) =>
                   Console.WriteLine($"Error: {e.Reason}"))
               .Build();
                messageTypeIsString = false;
            }
            else
                throw new KafkaConsumerUnsupportedMessageTypeException($"Message type '{TypeMessage.Name}' not supported");


            _consumer.Subscribe(_listenerConfiguration.TopicName);
            _logger = logger;

            _tagCunsumerGroupId = _metricService.CreateCustomTag(ConstValues.CONSUMER_GROUP_ID, _listenerConfiguration.GroupId);

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

            hostApplicationLifetime.ApplicationStopped.Register(() =>
            {
                _logger.LogWarning($"Consumer from topic {_listenerConfiguration.TopicName} shutdown");
            });
        }

        public async Task ExecuteAsync(string processId, CancellationToken stoppingToken)
        {
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

                        using var scope = _provider.CreateScope();
                        _isProcessing = true;

                        var msgKey = consume.Message.Key;
                        var msgBody = string.Empty;

                        if (messageTypeIsString)
                        {
                            msgBody = consume.Message.Value as string;
                        }
                        else
                        {
                            var value = consume.Message.Value as GenericRecord;
                            var json = value.ParseToJson();
                            msgBody = json.ToString();
                        }

                        var header = KafkaConsumerHelper.ParseHeader(consume.Message.Headers);
                        header.AddCurrentGroupId(_listenerConfiguration.GroupId);
                        header.AddCurrentTopicName(consume.Topic);
                        header.AddSourceTopicName(_listenerConfiguration.SourceTopicName);

                        var (consumerType, consumerLoggerType) = GetConsumerTypes(header, msgBody);
                        var consumerRegistered = consumerType == null ? null : scope.ServiceProvider.GetService(consumerType);
                        var logger = consumerLoggerType == null ? _logger : scope.ServiceProvider.GetService(consumerLoggerType) as ILogger;

                        var willRetry = false;
                        var watch = Stopwatch.StartNew();

                        var tagList = new List<KeyValuePair<string, object?>>() {
                            _tagCunsumerGroupId,
                            _metricService.CreateCustomTag(ConstValues.RETRY_MESSAGE, header.GetWillRetry().ToString()),
                            _metricService.CreateCustomTag(ConstValues.COMPANY_KEY, header.GetCompanykey()),
                            _metricService.CreateCustomTag(ConstValues.TOPIC_NAME, consume.Topic)
                        };

                        Activity activity = null;

                        try
                        {
                            if (consumerRegistered is null)
                            {
                                _consumer.Commit();
                                if (KafkaTelemetric.LogLevel > TelemetricLevel.Low)
                                {
                                    var msgSkipped = InternalLogMessage.Create("skipped", consume.Topic, consume.Partition, consume.Offset, header.GetCorrelationId());
                                    logger.LogWarning(msgSkipped.ToJson());
                                }
                                await MessageSkippedAsync(scope, msgKey, msgBody, header, stoppingToken);
                                tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, ConstValues.ConsumerMessageStatus.Skipped));
                            }
                            else
                            {
                                tagList.Add(_metricService.CreateCustomTag(ConstValues.CONSUMER_NAME, consumerType.Name));

                                var delay = header.GetRetryAt();
                                if (delay > 0)
                                    await Task.Delay(delay);

                                activity = GetActivity(consumerType, header, consume.Topic, msgKey);
                                var typeMessage = GetTypeMessage(consumerType, consumerRegistered);
                                var msgParsed = ParseMessage(typeMessage, msgBody);

                                BeforeConsume(consumerType, consumerRegistered, header, msgParsed);

                                try
                                {
                                    await ConsumeAsync(consumerType, consumerRegistered, header, msgParsed);
                                    tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, ConstValues.ConsumerMessageStatus.Success));
                                    if (KafkaTelemetric.LogLevel > TelemetricLevel.Medium)
                                    {
                                        var msg = InternalLogMessage.Create("success", consume.Topic, consume.Partition, consume.Offset, header.GetCorrelationId());
                                        logger.LogInformation(msg.ToJson());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    willRetry = await WillRetryAsync(msgKey, msgParsed, header, ex, stoppingToken);

                                    var errorMsg = InternalLogMessage.Create(ex.Message, consume.Topic, consume.Partition, consume.Offset, header.GetCorrelationId());
                                    if (willRetry)
                                    {
                                        if (KafkaTelemetric.LogLevel > TelemetricLevel.Low)
                                            logger.LogWarning(ex, errorMsg.ToJson());
                                    }
                                    else
                                        logger.LogError(ex, errorMsg.ToJson());

                                    throw ex;
                                }

                                AfterConsume(consumerType, consumerRegistered, header, msgParsed);
                                _consumer.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            var exception = ex.InnerException ?? ex;
                            var exceptionJson = JsonConvert.SerializeObject(exception);
                            activity?.SetStatus(ActivityStatusCode.Error, exceptionJson);
                            tagList.Add(_metricService.CreateCustomTag(ConstValues.EXCEPTION_FULL_NAME, exception.GetType().FullName));

                            bool shouldBeNotIgnored = ex.Message.Contains("No offset stored") is false;
                            if (shouldBeNotIgnored)
                            {
                                if (willRetry)
                                    tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, ConstValues.ConsumerMessageStatus.WillRetry));
                                else
                                    tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, ConstValues.ConsumerMessageStatus.Error));

                                header.AddWillRetry(willRetry);
                                _consumer.Commit();
                                await MessageErrorAsync(consumerType, consumerRegistered, consume.Topic, msgKey, msgBody, header, ex, stoppingToken);
                            }
                            else
                            {
                                if (KafkaTelemetric.LogLevel > TelemetricLevel.Low)
                                {
                                    var errorMsg = InternalLogMessage.Create(ex.Message, consume.Topic, consume.Partition, consume.Offset, header.GetCorrelationId());
                                    logger.LogWarning(ex, errorMsg.ToJson());
                                }
                                tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, ConstValues.ConsumerMessageStatus.Error));
                            }
                        }
                        finally
                        {
                            watch.Stop();
                            var elapsedTime = (int)watch.ElapsedMilliseconds;
                            _metricService.RecordConsumerElapsedTime(elapsedTime, tagList.ToArray());
                            activity?.Dispose();
                            _isProcessing = false;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Consumer from processId {processId} died, topic {_listenerConfiguration.TopicName} without consumer.");
                _consumer.Unassign();
                _consumer.Dispose();
                ConsumerErrorFatal(_provider.CreateScope(), ex);
                throw ex;
            }
        }

        private Activity? GetActivity(Type consumerType, HeaderValue header, string topicName, string messageKey)
        {
            var activityName = $"{consumerType.Name}_ConsumeAsync";
            var activity = _traceService.StartActivity(activityName, ActivityKind.Consumer, header.GetTraceId());

            if (activity is null)
                return activity;

            activity.SetTag(ConstValues.TOPIC_NAME, topicName);
            activity.SetTag(ConstValues.MESSAGE_KEY, messageKey);
            activity.SetTag(ConstValues.CORRELATION_ID, header.GetCorrelationId());
            activity.SetTag(ConstValues.COMPANY_KEY, header.GetCompanyKeyInternal());
            activity.SetTag(ConstValues.RETRY_MESSAGE, header.GetWillRetry().ToString());

            var currentAttempt = header.GetCurrentAttempt();
            if (currentAttempt > 0)
                activity.SetTag(ConstValues.RETRY_ATTEMPT, currentAttempt);

            return activity;
        }

        private (Type consumerType, Type consumerLoggerType) GetConsumerTypes(HeaderValue header, string msgBody)
        {
            var eventName = GetEventName(header, msgBody);

            var consumerKey = _listenerConfiguration.GetConsumerKey(eventName);
            var consumerType = Binds.GetType(consumerKey);

            var consumerLoggerKey = _listenerConfiguration.GetConsumerLoggerKey(eventName);
            var consumerLoggerType = Binds.GetType(consumerLoggerKey);

            if (consumerType == null)
            {
                consumerKey = _listenerConfiguration.GetConsumerKey(string.Empty);
                consumerType = Binds.GetType(consumerKey);
            }

            if (consumerLoggerType == null)
            {
                consumerLoggerKey = _listenerConfiguration.GetConsumerLoggerKey(string.Empty);
                consumerLoggerType = Binds.GetType(consumerLoggerKey);
            }

            return (consumerType, consumerLoggerType);
        }

        private string GetEventName(HeaderValue header, string msgBody)
        {
            try
            {
                if (header.GetIsNewClient())
                    return header.GetMessageName();
                else
                {
                    var msgParsed = ParseMessage(typeof(DefaultNotification), msgBody) as DefaultNotification;
                    if (msgParsed is null)
                        return header.GetMessageName();

                    return msgParsed.Name;
                }
            }
            catch
            {
                return header.GetMessageName();
            }
        }

        private async Task<bool> WillRetryAsync(string msgKey, object msgParsed, HeaderValue header, Exception ex, CancellationToken stoppingToken)
        {
            var retryConfig = _listenerConfiguration.RetryConfiguration;
            var currentAttempt = header.GetCurrentAttempt();
            var nextAttempt = currentAttempt + 1;
            var result = false;

            if (retryConfig != null)
            {
                try
                {
                    var retry = retryConfig.GetValidRetryTime(nextAttempt, ex);
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
                        headerRetry.AddWillRetry(true);
                        headerRetry.AddIsInternalProcess();
                        await _producerMessage.ProduceAsync(retryTopicName, key: msgKey, message: msgParsed, headerRetry, stoppingToken);
                        result = true;
                    }
                }
                catch
                {
                    result = false;
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
            errorFatal?.AlertError(ex);
        }


        private async Task MessageSkippedAsync(IServiceScope scope, string msgKey, string msgBody, HeaderValue header, CancellationToken stoppingToken)
        {
            var eventName = header.GetMessageName();

            if (string.IsNullOrEmpty(eventName))
            {
                var @event = ParseMessage(typeof(DefaultNotification), msgBody) as DefaultNotification;
                if (@event != null)
                    eventName = @event.Name;
            }

            if (_listenerConfiguration.GetIgnoreEvents.Contains(eventName) is false)
            {
                var context = ConsumeContext.Create(header);
                var skippedConsumer = scope.ServiceProvider.GetService<ISkippedMessage>();
                if (skippedConsumer != null)
                {
                    await skippedConsumer.AlertAsync(context, msgBody);
                }
                header.AddIsInternalProcess();
                var skipTopicName = BuilderName.GetTopicNameSkipped(_listenerConfiguration.GroupId, _listenerConfiguration.SourceTopicName);
                await _producerMessage.ProduceAsync(skipTopicName, key: msgKey, message: msgBody, header: header, stoppingToken);
            }
        }

        private async Task MessageErrorAsync(Type consumerType, object consumerClient, string topic, string msgKey, string msgBody, HeaderValue header, Exception ex, CancellationToken stoppingToken)
        {
            var context = ConsumeContext.Create(header);
            if (header.GetWillRetry() is false)
            {
                header.AddIsInternalProcess();
                var dlqTopicName = BuilderName.GetTopicNameDeadLetter(_listenerConfiguration.GroupId, topic);
                await _producerMessage.ProduceAsync(dlqTopicName, key: msgKey, message: new { MessageJson = msgBody, Error = ex }, header, stoppingToken);
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