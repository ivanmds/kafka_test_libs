using Bankly.Sdk.Kafka.Consumers;

namespace Performance_Consumer.Alerts
{
    public class CriticalErrorConsumer : IConsumerErrorFatal
    {
        private readonly ILogger<CriticalErrorConsumer> _logger;

        public CriticalErrorConsumer(ILogger<CriticalErrorConsumer> logger)
        {
            _logger = logger;
        }

        public void AlertError(Exception ex)
        {
            _logger.LogError(ex, "CriticalErrorConsumer_AlertError");
        }
    }
}
