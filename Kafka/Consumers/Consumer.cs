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

        public virtual void BeforeConsume(Context context, TMessage message) { }
        public abstract Task ConsumeAsync(Context context, TMessage message);
        public virtual void AfterConsume(Context context, TMessage message) { }
        public virtual void ErrorConsume(Exception ex) { }


        public Type GetTypeMessage()
        {
            return _typeMessage;
        }


    }
}
