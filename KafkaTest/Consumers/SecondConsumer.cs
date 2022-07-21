using Kafka.Consumers;
using KafkaTest.Models;

namespace KafkaTest.Consumers
{
    public class SecondConsumer : Consumer<Customer>
    {
        public override void Consume(Customer message)
        {
            Console.WriteLine(message);
        }
    }
}
