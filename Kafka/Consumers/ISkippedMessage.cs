using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Consumers
{
    public interface ISkippedMessage
    {
        Task AlertAsync(Context context, string message);
    }
}
