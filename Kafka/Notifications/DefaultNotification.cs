using System;
using System.Collections.Generic;

namespace Bankly.Sdk.Kafka.Notifications
{
    internal class DefaultNotification : IEventNotification<object>
    {
        public string EntityId { get; set; }

        public string CompanyKey { get; set; }

        public Context Context { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public object Data { get; set; }
    }
}
