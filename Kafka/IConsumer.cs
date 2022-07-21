namespace Kafka
{
    public interface IConsumer<TMessage> where TMessage : class
    {
        void Consume(TMessage message);
    }
}
