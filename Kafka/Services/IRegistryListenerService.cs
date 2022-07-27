using System.Collections.Generic;
using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka.Services
{
    internal interface IRegistryListenerService
    {
        void Add(string consumerKey, ListenerConfiguration listenerConfiguration);
        ListenerConfiguration Get(string consumerKey);
        IReadOnlyDictionary<string, ListenerConfiguration> GetListeners();
    }
}
