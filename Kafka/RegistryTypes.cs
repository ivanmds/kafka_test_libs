using System;
using System.Collections.Generic;

namespace Bankly.Sdk.Kafka
{
    internal static class RegistryTypes
    {
        private static readonly IDictionary<string, Type> _registerConsumer;

        static RegistryTypes()
        {
            _registerConsumer = new Dictionary<string, Type>();
        }

        public static void Register(string key, Type consumer)
        {
            if (_registerConsumer.ContainsKey(key))
                throw new ArgumentException($"The key {key} alreay registered");
            else
                _registerConsumer.Add(key, consumer);
        }

        public static Type? Recover(string key)
        {
            if (_registerConsumer.ContainsKey(key))
                return _registerConsumer[key];

            return null;
        }
    }
}
