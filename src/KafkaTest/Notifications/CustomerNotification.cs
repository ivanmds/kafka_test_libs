using Bankly.Sdk.Contracts.Enums;
using Bankly.Sdk.Contracts.Events;
using KafkaTest.Models;

namespace KafkaTest.Notifications
{
    public class CustomerNotification : IEventNotification<Customer>
    {
        public string EntityId { get; set; }

        public string CompanyKey { get; set; }

        public Context Context { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public Customer Data { get; set; }

        public Guid? IdempotencyKey { get; set; }

        public Guid? CorrelationId { get; set; }

        public IEnumerable<License>? Licenses { get; set; }

        public Guid? LicenseUuid { get; set; }

        public string Version { get; set; }
    }
}
