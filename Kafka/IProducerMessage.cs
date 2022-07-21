using System.Threading;
using System.Threading.Tasks;
using Kafka.Values;

namespace Kafka
{
    public interface IProducerMessage
    {
        Task ProduceAsync(string topicName, string key, object message, CancellationToken cancellationToken);

        Task ProduceAsync(string topicName, string key, object message, HeaderValue? header, CancellationToken cancellationToken);
    }
}
