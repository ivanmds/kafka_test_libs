using System;
using System.Collections.Generic;
using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka.Services
{
    internal class RegistryListenerService : IRegistryListenerService
    {
        private readonly Dictionary<string, ListenerConfiguration> _listeners;

        public RegistryListenerService()
        {
            _listeners = new Dictionary<string, ListenerConfiguration>();
        }

        public void Add(string consumerKey, ListenerConfiguration listenerConfiguration)
        {
            if (_listeners.ContainsKey(consumerKey))
                throw new ArgumentException($"Consumer {consumerKey} already was added.");

            _listeners.Add(consumerKey, listenerConfiguration);
        }

        public ListenerConfiguration Get(string consumerKey)
        {
            if (_listeners.ContainsKey(consumerKey) is false)
                throw new ArgumentException($"Consumer {consumerKey} not exist");

            return _listeners[consumerKey];
        }

        public IReadOnlyDictionary<string, ListenerConfiguration> GetListeners() => _listeners;
    }
}
