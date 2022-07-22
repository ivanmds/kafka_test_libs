using System.Threading;
using System.Threading.Tasks;
using Kafka.Notifications;
using Kafka.Values;

namespace Kafka
{
    public interface IProducerMessage
    {
        Task ProduceAsync(string topicName, string key, object message, CancellationToken cancellationToken);

        Task ProduceAsync(string topicName, string key, object message, HeaderValue? header = null, CancellationToken cancellationToken = default);

        Task ProduceAsync<T>(string topicName, string key, IEventNotification<T> eventMessage, HeaderValue? header = null, CancellationToken cancellationToken = default)
            where T : class;
    }
}
