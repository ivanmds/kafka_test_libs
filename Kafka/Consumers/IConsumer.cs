using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Consumers
{
    internal interface IConsumer<TMessage> where TMessage : class
    {
        Task ConsumeAsync(Context context, TMessage message);
    }
}