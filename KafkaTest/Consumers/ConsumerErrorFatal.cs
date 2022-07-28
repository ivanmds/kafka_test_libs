using Bankly.Sdk.Kafka.Consumers;

namespace KafkaTest.Consumers
{
    public class ConsumerErrorFatal : IConsumerErrorFatal
    {
        private readonly ILogger<ConsumerErrorFatal> _logger;

        public ConsumerErrorFatal(ILogger<ConsumerErrorFatal> logger)
        {
            _logger = logger;
        }

        public void AlertError(Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}
