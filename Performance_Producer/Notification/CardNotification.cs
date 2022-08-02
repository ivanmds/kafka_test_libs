using Bankly.Sdk.Kafka.Notifications;
using Performance_Producer.Models;

namespace Performance_Producer.Notification
{
    public class CardNotification : IEventNotification<Card>
    {
        public string EntityId { get; set; }

        public string CompanyKey { get; set; }

        public Context Context { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public Card Data { get; set; }
    }
}
