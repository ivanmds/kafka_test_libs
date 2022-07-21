using System;

namespace Kafka.Consumers
{
    internal interface IErrorConsume
    {
        void ErrorConsume(Exception ex);
    }
}
