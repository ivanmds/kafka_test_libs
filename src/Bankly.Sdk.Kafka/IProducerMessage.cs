using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Contracts.Events;
using Bankly.Sdk.Kafka.Values;

namespace Bankly.Sdk.Kafka
{
    public interface IProducerMessage
    {

        Task<ProduceResult> ProduceWithBindAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
           where TMessage : class;

        Task<ProduceResult> ProduceWithBindAsync<TMessage>(string key, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceWithBindAsync<TMessage>(TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceWithBindAsync<TMessage>(string key, TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
           where TMessage : class;



        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, string key, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, string key, TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;




        Task<ProduceResult> ProduceWithBindNotificationAsync<TMessage>(string key, IEventNotification<TMessage> eventMessage, CancellationToken cancellationToken = default)
           where TMessage : class;

        Task<ProduceResult> ProduceWithBindNotificationAsync<TMessage>(string key, IEventNotification<TMessage> eventMessage, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;



        Task<ProduceResult> ProduceNotificationAsync<TMessage>(string topicName, string key, IEventNotification<TMessage> eventMessage, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceNotificationAsync<TMessage>(string topicName, string key, IEventNotification<TMessage> eventMessage, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;
        
    }
}