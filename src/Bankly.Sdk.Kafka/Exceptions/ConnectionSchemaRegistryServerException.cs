using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    internal class ConnectionSchemaRegistryServerException : Exception
    {
        public ConnectionSchemaRegistryServerException(string message) : base(message)
        {

        }
    }
}
