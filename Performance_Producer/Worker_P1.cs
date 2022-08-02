using Bankly.Sdk.Kafka;
using Performance_Producer.Factory;

namespace Performance_Producer
{
    public class Worker_P1 : BackgroundService
    {
        private readonly ILogger<Worker_P1> _logger;
        private readonly IProducerMessage _producerMessage;

        public Worker_P1(ILogger<Worker_P1> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var customer = CustomerFactory.GetCustomer();
                await _producerMessage.ProduceWithBindAsync(customer, stoppingToken);
                await Task.Delay(10, stoppingToken);
            }
        }
    }
}