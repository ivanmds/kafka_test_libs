namespace Kafka.Consumers
{
    internal interface IConsumer<TMessage> where TMessage : class
    {
        void Consume(TMessage message);
    }
}