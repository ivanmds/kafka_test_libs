namespace Kafka.Consumers
{
    internal interface IBeforeConsume<TMessage> where TMessage : class
    {
        void BeforeConsume(Context context, TMessage message);
    }
}
