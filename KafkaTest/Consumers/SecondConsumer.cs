using Kafka.Consumers;
using KafkaTest.Models;

namespace KafkaTest.Consumers
{
    public class SecondConsumer : Consumer<Customer>
    {
        public override async Task ConsumeAsync(Customer message)
        {
            Console.WriteLine(message);
        }
    }
}
