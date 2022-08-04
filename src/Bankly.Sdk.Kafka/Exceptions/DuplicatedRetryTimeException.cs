using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    public class DuplicatedRetryTimeException : Exception
    {
        public DuplicatedRetryTimeException(string message) : base(message)
        {

        }
    }
}
