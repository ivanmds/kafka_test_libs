using Bankly.Sdk.Kafka.Consumers;
using Performance_Consumer.Models;

namespace Performance_Consumer
{
    public class CardConsumer : Consumer<Card>
    {
        private readonly ILogger<CardConsumer> _logger;

        public CardConsumer(ILogger<CardConsumer> logger)
        {
            _logger = logger;
        }

        public override async Task ConsumeAsync(ConsumeContext context, Card message)
        {
            //_logger.LogInformation($"CardConsumer proxy: {message.Proxy}");
            await Task.Delay(150);
        }

        public override void ErrorConsume(ConsumeContext context, Exception ex)
        {
            _logger.LogError(ex, "CardConsumer_ErrorConsume");
        }
    }
}
