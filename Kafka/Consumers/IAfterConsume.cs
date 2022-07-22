namespace Kafka.Consumers
{
    internal interface IAfterConsume<TMessage> where TMessage : class
    {
        void AfterConsume(Context context, TMessage message);
    }
}
