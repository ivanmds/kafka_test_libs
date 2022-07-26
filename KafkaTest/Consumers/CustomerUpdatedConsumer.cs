using Bankly.Sdk.Kafka.Consumers;
using KafkaTest.Notifications;

namespace KafkaTest.Consumers
{
    public class CustomerUpdatedConsumer : Consumer<CustomerNotification>
    {
        public override async Task ConsumeAsync(Context context, CustomerNotification message)
        {
            throw new Exception("Test");
        }

        public override void ErrorConsume(Context context, Exception ex)
        {
            base.ErrorConsume(context, ex);
        }
    }
}
