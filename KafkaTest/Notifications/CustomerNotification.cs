using Bankly.Sdk.Kafka.Notifications;
using KafkaTest.Models;

namespace KafkaTest.Notifications
{
    public class CustomerNotification : IEventNotification<Customer>
    {
        public string EntityId { get; set; }

        public string CompanyKey { get; set; }

        public string Context { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public Customer Data { get; set; }
    }
}
