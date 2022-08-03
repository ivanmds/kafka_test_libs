using Bankly.Sdk.Kafka.Consumers;
using KafkaTest.Notifications;

namespace KafkaTest.Consumers
{
    public class CustomerCreatedConsumer : Consumer<CustomerNotification>
    {
        private readonly ILogger<CustomerCreatedConsumer> _logger;

        public CustomerCreatedConsumer(ILogger<CustomerCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public override void BeforeConsume(ConsumeContext context, CustomerNotification message)
        {
            Console.WriteLine("before simple consumer " + message.Name);
        }

        public override async Task ConsumeAsync(ConsumeContext context, CustomerNotification message)
        {
            _logger.LogInformation("simple consumer " + message.Name);
        }

        public override void AfterConsume(ConsumeContext context, CustomerNotification message)
        {
            Console.WriteLine("after simple consumer " + message.Name);
        }

        public override void ErrorConsume(ConsumeContext context, Exception ex)
        {
            Console.WriteLine("error: " + ex.Message);
        }
    }
}
