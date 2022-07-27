using Bankly.Sdk.Kafka.Consumers;
using KafkaTest.Notifications;

namespace KafkaTest.Consumers
{
    public class CardProgramWasCreated : Consumer<CardNotification>
    {
        private readonly ILogger<CardWasIssuedConsumer> _logger;
        public CardProgramWasCreated(ILogger<CardWasIssuedConsumer> logger)
        {
            _logger = logger;
        }

        public override async Task ConsumeAsync(ConsumeContext context, CardNotification message)
        {
            _logger.LogInformation($"Was processed {message.Name} with proxy {message.Data.Proxy}");
        }
    }
}
