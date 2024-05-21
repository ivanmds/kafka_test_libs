using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    internal class KafkaConsumerUnsupportedMessageTypeException : Exception
    {
        public KafkaConsumerUnsupportedMessageTypeException(string message) : base(message)
        {

        }
    }
}
