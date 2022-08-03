using Bankly.Sdk.Kafka.Consumers;
using Performance_Consumer.Models;

namespace Performance_Consumer
{
    public class CustomerConsumer : Consumer<Customer>
    {
        private readonly ILogger<CustomerConsumer> _logger;
        public CustomerConsumer(ILogger<CustomerConsumer> logger)
        {
            _logger = logger;
        }

        public override async Task ConsumeAsync(ConsumeContext context, Customer message)
        {
            _logger.LogInformation($"CustomerConsumer  doc: {message.DocumentNumber}");
            await Task.Delay(150);
        }
    }
}
