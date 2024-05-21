using System;
using System.Collections.Generic;
using Bankly.Sdk.Contracts.Enums;
using Bankly.Sdk.Contracts.Events;

namespace Bankly.Sdk.Kafka.Notifications
{
    internal class DefaultNotification : IEventNotification<object>
    {
        public string EntityId { get; set; }

        public string CompanyKey { get; set; }

        public Context Context { get; set; }

        public string Name { get; set; }

        public string Version { get; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public object Data { get; set; }

        public Guid? IdempotencyKey { get; set; }

        public Guid? CorrelationId { get; set; }

        public IEnumerable<License>? Licenses { get; set; }

        public Guid? LicenseUuid { get; set; }
    }
}
