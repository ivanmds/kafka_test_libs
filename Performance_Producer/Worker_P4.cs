using Bankly.Sdk.Kafka;
using Performance_Producer.Notification;

namespace Performance_Producer
{
    internal class Worker_P4 : BackgroundService
    {
        private readonly ILogger<Worker_P4> _logger;
        private readonly IProducerMessage _producerMessage;

        public Worker_P4(ILogger<Worker_P4> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var cardEvent = new CardNotification { EntityId = Guid.NewGuid().ToString() };  
                await _producerMessage.ProduceWithBindNotificationAsync(cardEvent.EntityId, cardEvent, stoppingToken);
                await Task.Delay(10, stoppingToken);
            }
        }
    }
}
