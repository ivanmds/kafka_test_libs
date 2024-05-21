using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Contracts.Events;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Contracts;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Exceptions;
using Bankly.Sdk.Kafka.Metrics;
using Bankly.Sdk.Kafka.Traces;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.Clients
{
    internal class ProducerMessage : IProducerMessage, IDisposable
    {
        private readonly IProducer<string, string> _kafkaProducer;
        private readonly IMetricService _metricService;
        private readonly ITraceService _traceService;
        private readonly ILogger<ProducerMessage> _logger;
        private bool _disposedValue;

        public ProducerMessage(KafkaConnection kafkaConnection, IMetricService metricService, ITraceService traceService, ILogger<ProducerMessage> logger)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaConnection.BootstrapServers,
                SecurityProtocol = kafkaConnection.IsPlaintext ? SecurityProtocol.Plaintext : SecurityProtocol.Ssl
            };
            _metricService = metricService;
            _traceService = traceService;
            _logger = logger;
            _kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task<ProduceResult> ProduceWithBindAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
            => await ProduceMessageAsync(null, null, message, null, cancellationToken);

        public async Task<ProduceResult> ProduceWithBindAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default) where TMessage : class
            => await ProduceMessageAsync(null, key, message, null, cancellationToken);

        public async Task<ProduceResult> ProduceWithBindAsync<TMessage>(TMessage message, HeaderValue header, CancellationToken cancellationToken = default) where TMessage : class
            => await ProduceMessageAsync(null, null, message, header, cancellationToken);

        public async Task<ProduceResult> ProduceWithBindAsync<TMessage>(string key, TMessage message, HeaderValue header, CancellationToken cancellationToken = default) where TMessage : class
             => await ProduceMessageAsync(null, key, message, header, cancellationToken);

        public async Task<ProduceResult> ProduceAsync<TMessage>(string topicName, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class
            => await ProduceMessageAsync(topicName, null, message, null, cancellationToken);

        public async Task<ProduceResult> ProduceAsync<TMessage>(string topicName, string key, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class
            => await ProduceMessageAsync(topicName, key, message, null, cancellationToken);

        public async Task<ProduceResult> ProduceAsync<TMessage>(string topicName, TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class
            => await ProduceMessageAsync(topicName, null, message, header, cancellationToken);

        public async Task<ProduceResult> ProduceAsync<TMessage>(string topicName, string key, TMessage message, HeaderValue header, CancellationToken cancellationToken = default) where TMessage : class
            => await ProduceMessageAsync(topicName, key, message, header, cancellationToken);


        public async Task<ProduceResult> ProduceWithBindNotificationAsync<TMessage>(string key, IEventNotification<TMessage> eventMessage, CancellationToken cancellationToken = default) where TMessage : class
            => await ProduceMessageAsync(null, key, eventMessage, null, cancellationToken);

        public async Task<ProduceResult> ProduceWithBindNotificationAsync<TMessage>(string key, IEventNotification<TMessage> eventMessage, HeaderValue header, CancellationToken cancellationToken = default) where TMessage : class
            => await ProduceMessageAsync(null, key, eventMessage, header, cancellationToken);


        public async Task<ProduceResult> ProduceNotificationAsync<TMessage>(string topicName, string key, IEventNotification<TMessage> eventMessage, CancellationToken cancellationToken = default)
            where TMessage : class
            => await ProduceMessageAsync(topicName, key, eventMessage, null, cancellationToken);

        public async Task<ProduceResult> ProduceNotificationAsync<TMessage>(string topicName, string key, IEventNotification<TMessage> eventMessage, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class
            => await ProduceMessageAsync(topicName, key, eventMessage, header, cancellationToken);


        private async Task<ProduceResult> ProduceMessageAsync<TMessage>(string? topicName, string? key, TMessage message, HeaderValue? header, CancellationToken cancellationToken)
           where TMessage : class
        {
            var tagList = GetTagList(false, header?.GetCompanyKeyInternal() ?? "none", header?.GetIsInternalProcess());
            var watch = Stopwatch.StartNew();

            Activity activity = GetProduceActivity();

            try
            {
                if(topicName is null)
                {
                    var messageFullName = typeof(TMessage).FullName;
                    topicName = Binds.GetString(messageFullName);
                    if(topicName == null)
                        throw new NoneBindConfiguredException(messageFullName);
                }

                if(topicName.StartsWith("bankly.event"))
                    throw new InvalidTopicNameException("Should be used the method to IEventNotification");

                var messageNotification = message.GetType() == typeof(string)
                  ? message.ToString()
                  : JsonConvert.SerializeObject(message, DefaultSerializerSettings.JsonSettings);

                var kafkaMessage = new Message<string, string> { Value = messageNotification };
                if(string.IsNullOrEmpty(key) is false)
                    kafkaMessage.Key = key;

                header ??= new HeaderValue();
                header.AddIsNewClient();
                header.AddCurrentTraceId();

                SetDefaultTraceTags(activity, topicName, header);

                if(message is IMessage)
                    header.AddCommandName(((IMessage)message).MessageName ?? "");

                if(header != null)
                {
                    kafkaMessage.Headers = new Headers();

                    foreach(var kv in header.GetKeyValues())
                    {
                        var valueBytes = Encoding.ASCII.GetBytes(kv.Value);
                        var msgHeader = new Header(kv.Key, valueBytes);
                        kafkaMessage.Headers.Add(msgHeader);
                    }
                }

                var result = await _kafkaProducer.ProduceAsync(topicName, kafkaMessage, cancellationToken);

                if(KafkaTelemetric.LogLevel > TelemetricLevel.Medium)
                {
                    var msg = InternalLogMessage.Create("producer", result.Topic, result.Partition, result.Offset, header.GetCorrelationId());
                    _logger.LogInformation(msg.ToJson());
                }

                var produceResult = ProduceResult.Create(result.Status == PersistenceStatus.Persisted);
                var tagValue = produceResult.IsSuccess ? ConstValues.ProducerMessageStatus.Success : ConstValues.ProducerMessageStatus.Unsuccess;
                tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, tagValue));
                return produceResult;
            }
            catch(Exception ex)
            {
                SetTagWhenError(ex, activity, tagList);
                throw ex;
            }
            finally
            {
                watch.Stop();
                var elapsedTime = (int)watch.ElapsedMilliseconds;
                _metricService.RecordProducerElapsedTime(elapsedTime, tagList.ToArray());

                activity?.Dispose();
            }
        }

        private Activity GetProduceActivity()
        {
            if(KafkaTelemetric.TraceLevel > TelemetricLevel.Medium)
                return _traceService.StartActivity("IProducerMessage.ProduceNotification", ActivityKind.Producer);

            return null;
        }

        private async Task<ProduceResult> ProduceMessageAsync<TMessage>(string? topicName, string key, IEventNotification<TMessage> eventMessage, HeaderValue? header, CancellationToken cancellationToken)
           where TMessage : class
        {
            var tagList = GetTagList(true, header?.GetCompanyKeyInternal() ?? "none", header?.GetIsInternalProcess());
            var watch = Stopwatch.StartNew();

            Activity activity = GetProduceActivity();

            try
            {
                if(string.IsNullOrEmpty(key))
                    throw new InvalidProgramException("Should be informed the message key.");

                if(topicName is null)
                {
                    var messageFullName = eventMessage.GetType().FullName;
                    topicName = Binds.GetString(messageFullName);
                    if(topicName == null)
                        throw new NoneBindConfiguredException("Make bind of message with topicName");
                }

                if(!topicName.StartsWith("bankly.event"))
                    throw new InvalidTopicNameException("The topic name should be started with bankly.event");

                var messageNotification = JsonConvert.SerializeObject(eventMessage, DefaultSerializerSettings.JsonSettings);
                var kafkaMessage = new Message<string, string> { Key = key, Value = messageNotification };

                header ??= new HeaderValue();
                header.AddIsNewClient();
                header.AddIsNotification();
                header.AddEventName(eventMessage.Name ?? "");

                SetDefaultTraceTags(activity, topicName, header);

                kafkaMessage.Headers = new Headers();

                foreach(var kv in header.GetKeyValues())
                {
                    var valueBytes = Encoding.ASCII.GetBytes(kv.Value);
                    var msgHeader = new Header(kv.Key, valueBytes);
                    kafkaMessage.Headers.Add(msgHeader);
                }

                var result = await _kafkaProducer.ProduceAsync(topicName, kafkaMessage, cancellationToken);

                if(KafkaTelemetric.LogLevel > TelemetricLevel.Medium)
                {
                    var msg = InternalLogMessage.Create("producer notification", result.Topic, result.Partition, result.Offset, header.GetCorrelationId());
                    _logger.LogInformation(msg.ToJson());
                }

                var produceResult = ProduceResult.Create(result.Status == PersistenceStatus.Persisted);

                var tagValue = produceResult.IsSuccess ? ConstValues.ProducerMessageStatus.Success : ConstValues.ProducerMessageStatus.Unsuccess;
                tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, tagValue));
                return produceResult;
            }
            catch(Exception ex)
            {
                SetTagWhenError(ex, activity, tagList);
                throw ex;
            }
            finally
            {
                watch.Stop();
                var elapsedTime = (int)watch.ElapsedMilliseconds;
                _metricService.RecordProducerElapsedTime(elapsedTime, tagList.ToArray());

                activity?.Dispose();
            }
        }

        private void SetDefaultTraceTags(Activity activity, string topicName, HeaderValue header)
        {
            if(activity != null)
            {
                activity.SetTag(ConstValues.TOPIC_NAME, topicName);
                activity.SetTag(ConstValues.CORRELATION_ID, header.GetCorrelationId());
                activity.SetTag(ConstValues.COMPANY_KEY, header.GetCompanyKeyInternal());
            }
        }

        private void SetTagWhenError(Exception ex, Activity activity, List<KeyValuePair<string, object?>> tagList)
        {
            var exception = ex.InnerException ?? ex;
            var exceptionJson = JsonConvert.SerializeObject(exception);
            activity?.SetStatus(ActivityStatusCode.Error, exceptionJson);
            _logger.LogError(ex, ex.Message);
            tagList.Add(_metricService.CreateCustomTag(ConstValues.EXCEPTION_FULL_NAME, exception.GetType().FullName));
            tagList.Add(_metricService.CreateCustomTag(ConstValues.MESSAGE_STATUS, ConstValues.ProducerMessageStatus.Error));
        }

        private List<KeyValuePair<string, object?>> GetTagList(bool isNotification, string companyKey, string valueInternalProcess)
        {
            var tagList = new List<KeyValuePair<string, object?>>() {
                _metricService.CreateCustomTag(ConstValues.MESSAGE_IS_NOTIFICATION, isNotification.ToString()),
                _metricService.CreateCustomTag(ConstValues.COMPANY_KEY, companyKey),
                _metricService.CreateCustomTag(ConstValues.IS_INTERNAL_PROCESS, valueInternalProcess)
            };

            return tagList;
        }

        ~ProducerMessage() => Dispose(true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!_disposedValue)
            {
                if(disposing)
                {
                    _kafkaProducer.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
