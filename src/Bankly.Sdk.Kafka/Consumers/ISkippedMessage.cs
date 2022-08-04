using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Consumers
{
    public interface ISkippedMessage
    {
        Task AlertAsync(ConsumeContext context, string message);
    }
}
