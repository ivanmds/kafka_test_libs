namespace Kafka.Consumers
{
    internal interface IAfterConsume<TMessage> where TMessage : class
    {
        void AfterConsume(TMessage message);
    }
}
