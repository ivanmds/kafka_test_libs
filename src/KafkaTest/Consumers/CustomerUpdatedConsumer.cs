using Bankly.Sdk.Kafka.Consumers;
using KafkaTest.Notifications;

namespace KafkaTest.Consumers
{
    public class CustomerUpdatedConsumer : Consumer<CustomerNotification>
    {
        private readonly ILogger<CustomerUpdatedConsumer> _logger;
        public CustomerUpdatedConsumer(ILogger<CustomerUpdatedConsumer> logger)
        {
            _logger = logger;
        }

        public override async Task ConsumeAsync(ConsumeContext context, CustomerNotification message)
        {
            throw new Exception("Test");
        }

        public override void ErrorConsume(ConsumeContext context, Exception ex)
        {
            _logger.LogWarning(ex.Message);

            base.ErrorConsume(context, ex);
        }
    }
}
