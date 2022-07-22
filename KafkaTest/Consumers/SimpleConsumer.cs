using Kafka.Consumers;
using KafkaTest.Models;

namespace KafkaTest.Consumers
{
    public class SimpleConsumer : Consumer<Customer>
    {
        private readonly ILogger<SimpleConsumer> _logger;

        public SimpleConsumer(ILogger<SimpleConsumer> logger)
        {
            _logger = logger;
        }

        public override void BeforeConsume(Context context, Customer message)
        {
            Console.WriteLine("before simple consumer " + message.Name);
        }

        public override async Task ConsumeAsync(Context context, Customer message)
        {
            _logger.LogInformation("simple consumer " + message.Name);
        }

        public override void AfterConsume(Context context, Customer message)
        {
            Console.WriteLine("after simple consumer " + message.Name);
        }

        public override void ErrorConsume(Exception ex)
        {
            Console.WriteLine("error: " + ex.Message);
        }
    }
}
