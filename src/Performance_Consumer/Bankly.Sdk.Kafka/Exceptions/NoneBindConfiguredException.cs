using System;

namespace Bankly.Sdk.Kafka.Exceptions
{
    public class NoneBindConfiguredException : Exception
    {
        public NoneBindConfiguredException(string contractName)
            : base($"None bind finded to contract name {contractName}, consider config a bind before.") { }
    }
}