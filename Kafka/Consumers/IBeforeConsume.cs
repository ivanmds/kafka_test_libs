namespace Bankly.Sdk.Kafka.Consumers
{
    internal interface IBeforeConsume<TMessage> where TMessage : class
    {
        void BeforeConsume(ConsumeContext context, TMessage message);
    }
}
