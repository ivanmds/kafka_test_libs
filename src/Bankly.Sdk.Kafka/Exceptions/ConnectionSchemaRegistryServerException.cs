using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    internal class ConnectionSchemaRegistryServerException : Exception
    {
        public ConnectionSchemaRegistryServerException(string message = "Should be informed url to cluster schema registry") : base(message)
        {
        }
    }
}
