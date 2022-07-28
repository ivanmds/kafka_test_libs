using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Notifications;
using Bankly.Sdk.Kafka.Values;
using Newtonsoft.Json;
using System;

namespace Bankly.Sdk.Kafka.Clients
{
    public class KafkaClient : IKafkaClient, IDisposable
    {
        private readonly IProducer<string, string> _kafkaProducer;
        private bool _disposedValue;

        public KafkaClient(KafkaConnection kafkaConnection)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaConnection.BootstrapServers,
                SecurityProtocol = kafkaConnection.IsPlaintext ? SecurityProtocol.Plaintext : SecurityProtocol.Ssl
            };

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
            if (topicName is null)
            {
                var messageFullName = typeof(TMessage).FullName;
                topicName = Binds.GetString(messageFullName);
                if (topicName == null)
                    throw new Exception("Make bind of message with topicName");
            }


            if (topicName.StartsWith("bankly.event"))
                throw new Exception("Should be used the method to IEventNotification");

            var messageNotification = JsonConvert.SerializeObject(message, DefaultSerializerSettings.JsonSettings);

            var kafkaMessage = new Message<string, string> { Value = messageNotification };
            if (string.IsNullOrEmpty(key) is false)
                kafkaMessage.Key = key;

            header ??= new HeaderValue();
            header.AddIsNewClient();

            if (header != null)
            {
                kafkaMessage.Headers = new Headers();

                foreach (var kv in header.GetKeyValues())
                {
                    var valueBytes = Encoding.ASCII.GetBytes(kv.Value);
                    var msgHeader = new Header(kv.Key, valueBytes);
                    kafkaMessage.Headers.Add(msgHeader);
                }
            }

            var result = await _kafkaProducer.ProduceAsync(topicName, kafkaMessage, cancellationToken);

            return ProduceResult.Create(result.Status == PersistenceStatus.Persisted);
        }

        private async Task<ProduceResult> ProduceMessageAsync<TMessage>(string? topicName, string key, IEventNotification<TMessage> eventMessage, HeaderValue? header, CancellationToken cancellationToken)
           where TMessage : class
        {
            if(string.IsNullOrEmpty(key))
                throw new Exception("Should be informed the message key.");

            if (topicName is null)
            {
                var messageFullName = eventMessage.GetType().FullName;
                topicName = Binds.GetString(messageFullName);
                if (topicName == null)
                    throw new Exception("Make bind of message with topicName");
            }

            if (!topicName.StartsWith("bankly.event"))
                throw new Exception("The topic name should be started with bankly.event");

            var messageNotification = JsonConvert.SerializeObject(eventMessage, DefaultSerializerSettings.JsonSettings);
            var kafkaMessage = new Message<string, string> { Key = key, Value = messageNotification };

            header ??= new HeaderValue();
            header.AddIsNewClient();
            header.AddIsNotification();
            header.AddEventName(eventMessage.Name ?? "");

            kafkaMessage.Headers = new Headers();

            foreach (var kv in header.GetKeyValues())
            {
                var valueBytes = Encoding.ASCII.GetBytes(kv.Value);
                var msgHeader = new Header(kv.Key, valueBytes);
                kafkaMessage.Headers.Add(msgHeader);
            }


            var result = await _kafkaProducer.ProduceAsync(topicName, kafkaMessage, cancellationToken);
            return ProduceResult.Create(result.Status == PersistenceStatus.Persisted);
        }

        
        ~KafkaClient() => Dispose(false);

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _kafkaProducer.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
