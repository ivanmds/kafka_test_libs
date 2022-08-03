using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Consumers
{
    internal interface IConsumer<TMessage> where TMessage : class
    {
        Task ConsumeAsync(ConsumeContext context, TMessage message);
    }
}