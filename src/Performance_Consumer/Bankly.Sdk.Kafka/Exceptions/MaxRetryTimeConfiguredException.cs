using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    public class MaxRetryTimeConfiguredException : Exception
    {
        public MaxRetryTimeConfiguredException(string message) : base(message)
        {
        }
    }
}
