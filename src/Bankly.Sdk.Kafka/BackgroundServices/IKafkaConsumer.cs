using System.Threading.Tasks;
using System.Threading;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal interface IKafkaConsumer
    {
        Task ExecuteAsync(string processId, CancellationToken stoppingToken);
    }
}
