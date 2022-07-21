namespace Kafka.Consumers
{
    internal interface IBeforeConsume<TMessage> where TMessage : class
    {
        void BeforeConsume(TMessage message);
    }
}
