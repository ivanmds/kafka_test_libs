using System;
using System.Collections.Generic;

namespace Bankly.Sdk.Kafka.Notifications
{
    public interface IEventNotification<out TData> where TData : class
    {
        string EntityId { get; }
        string CompanyKey { get; }
        string Context { get; }
        string Name { get; }
        DateTime Timestamp { get; }
        IDictionary<string, object> Metadata { get; }
        TData Data { get; }
    }
}
