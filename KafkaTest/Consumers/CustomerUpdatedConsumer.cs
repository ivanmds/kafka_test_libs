using Kafka.Consumers;
using KafkaTest.Notifications;

namespace KafkaTest.Consumers
{
    public class CustomerUpdatedConsumer : Consumer<CustomerNotification>
    {
        public override async Task ConsumeAsync(Context context, CustomerNotification message)
        {
            Console.WriteLine(message);
        }
    }
}
