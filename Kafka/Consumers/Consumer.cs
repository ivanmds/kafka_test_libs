using System;
using System.Threading.Tasks;

namespace Kafka.Consumers
{
    public abstract class Consumer<TMessage> : IConsumer<TMessage>, IBeforeConsume<TMessage>, IAfterConsume<TMessage>, IErrorConsume
        where TMessage : class
    {
        private readonly Type _typeMessage;

        public Consumer()
        {
            _typeMessage = typeof(TMessage);
        }

        public virtual void BeforeConsume(TMessage message) { }

        public abstract Task ConsumeAsync(TMessage message);

        public virtual void AfterConsume(TMessage message) { }

        public virtual void ErrorConsume(Exception ex) { }

        public Type GetTypeMessage()
        {
            return _typeMessage;
        } 
    }
}
