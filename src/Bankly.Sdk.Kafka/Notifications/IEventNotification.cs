using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bankly.Sdk.Kafka.Notifications
{
    public interface IEventNotification<out TData> where TData : class
    {
        string EntityId { get; }
        string CompanyKey { get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        Context Context { get; }

        string Name { get; }
        DateTime Timestamp { get; }
        IDictionary<string, object> Metadata { get; }
        TData Data { get; }
    }
}
