using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    public class InvalidTopicNameException : Exception
    {
        public InvalidTopicNameException(string message) : base(message)
        {

        }
    }
}
