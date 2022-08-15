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
            _logger.LogInformation($"Init CustomerConsumer  doc: {message.DocumentNumber}");
            await Task.Delay(60000);
            _logger.LogInformation($"Finish CustomerConsumer  doc: {message.DocumentNumber}");
        }

        public override void ErrorConsume(ConsumeContext context, Exception ex)
        {
            _logger.LogError(ex, "CustomerConsumer_ErrorConsume");
        }
    }
}
