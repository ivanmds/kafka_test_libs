using Bankly.Sdk.Kafka;
using Performance_Producer.Factory;

namespace Performance_Producer
{
    internal class Worker_P3 : BackgroundService
    {
        private readonly ILogger<Worker_P3> _logger;
        private readonly IProducerMessage _producerMessage;

        public Worker_P3(ILogger<Worker_P3> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var customerEvent = CustomerFactory.GetCustomerNotification();
                await _producerMessage.ProduceWithBindNotificationAsync(customerEvent.EntityId, customerEvent, stoppingToken);
                await Task.Delay(10, stoppingToken);
            }
        }
    }
}
