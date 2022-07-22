using System.Threading;
using System.Threading.Tasks;
using Kafka.Notifications;
using Kafka.Values;

namespace Kafka
{
    public interface IProducerMessage
    {
        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, string key, TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceAsync<TMessage>(string topicName, string key, TMessage message, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceNotificationAsync<TMessage>(string topicName, string key, IEventNotification<TMessage> eventMessage, CancellationToken cancellationToken = default)
            where TMessage : class;

        Task<ProduceResult> ProduceNotificationAsync<TMessage>(string topicName, string key, IEventNotification<TMessage> eventMessage, HeaderValue header, CancellationToken cancellationToken = default)
            where TMessage : class;        
    }
}