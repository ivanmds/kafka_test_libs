using System;
using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Consumers
{
    public abstract class Consumer<TMessage> : IConsumer<TMessage>, IBeforeConsume<TMessage>, IAfterConsume<TMessage>, IErrorConsume
        where TMessage : class
    {
        private readonly Type _typeMessage;

        public Consumer()
        {
            _typeMessage = typeof(TMessage);
        }

        public virtual void BeforeConsume(ConsumeContext context, TMessage message) { }
        public abstract Task ConsumeAsync(ConsumeContext context, TMessage message);
        public virtual void AfterConsume(ConsumeContext context, TMessage message) { }
        public virtual void ErrorConsume(ConsumeContext context, Exception ex) { }

        public Type GetTypeMessage()
        {
            return _typeMessage;
        }
    }
}
