using Bankly.Sdk.Kafka.Consumers;
using Performance_Consumer.Notification;

namespace Performance_Consumer
{
    public class CustomerNotificationConsumer : Consumer<CustomerNotification>
    {
        private readonly ILogger<CustomerNotificationConsumer> _logger;
        public CustomerNotificationConsumer(ILogger<CustomerNotificationConsumer> logger)
        {
            _logger = logger;
        }

        public override async Task ConsumeAsync(ConsumeContext context, CustomerNotification message)
        {
            _logger.LogInformation($"CustomerNotificationConsumer id:{message.EntityId}");
            await Task.Delay(70);
        }
    }
}
