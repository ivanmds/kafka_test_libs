namespace Bankly.Sdk.Kafka.Consumers
{
    internal interface IAfterConsume<TMessage> where TMessage : class
    {
        void AfterConsume(ConsumeContext context, TMessage message);
    }
}
