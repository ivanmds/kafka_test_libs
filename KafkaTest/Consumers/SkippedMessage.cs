using Bankly.Sdk.Kafka.Consumers;

namespace KafkaTest.Consumers
{
    public class SkippedMessage : ISkippedMessage
    {
        private readonly ILogger<SkippedMessage> _logger;
        public SkippedMessage(ILogger<SkippedMessage> logger)
        {
            _logger = logger;
        }

        public async Task AlertAsync(ConsumeContext context, string message)
        {
            _logger.LogInformation($"Skipped message: {message}");
        }
    }
}
