using Bankly.Sdk.Kafka.Notifications;
using Performance_Consumer.Models;

namespace Performance_Consumer.Notification
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
    }
}
