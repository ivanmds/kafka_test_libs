using Kafka.Consumers;
using KafkaTest.Models;

namespace KafkaTest.Consumers
{
    public class AnotherConsumer : Consumer<Customer>
    {
        public override async Task ConsumeAsync(Context context, Customer message)
        {
            Console.WriteLine(message);
        }
    }
}
