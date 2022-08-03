using System;

namespace Bankly.Sdk.Kafka.Consumers
{
    internal interface IErrorConsume
    {
        void ErrorConsume(ConsumeContext context, Exception ex);
    }
}
