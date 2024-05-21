using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    public class WhenRetryConflitException : Exception
    {
        public WhenRetryConflitException(string message) : base(message)
        {
        }
    }
}
