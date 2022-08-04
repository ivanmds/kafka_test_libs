using System;

namespace Bankly.Sdk.Kafka.Consumers
{
    public interface IConsumerErrorFatal
    {
        void AlertError(Exception ex);
    }
}
