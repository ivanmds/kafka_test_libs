using Bankly.Sdk.Kafka;
using Performance_Producer.Models;

namespace Performance_Producer
{
    internal class Worker_P2 : BackgroundService
    {
        private readonly ILogger<Worker_P2> _logger;
        private readonly IProducerMessage _producerMessage;

        public Worker_P2(ILogger<Worker_P2> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var card = new Card { Proxy = Guid.NewGuid().ToString() };
                await _producerMessage.ProduceWithBindAsync(card, stoppingToken);
                await Task.Delay(10, stoppingToken);
            }
        }
    }
}
