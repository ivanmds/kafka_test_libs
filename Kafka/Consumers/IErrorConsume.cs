using System;

namespace Bankly.Sdk.Kafka.Consumers
{
    internal interface IErrorConsume
    {
        void ErrorConsume(Context context, Exception ex);
    }
}
