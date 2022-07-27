using Bankly.Sdk.Kafka.Consumers;
using KafkaTest.Models;

namespace KafkaTest.Consumers
{
    public class AnotherConsumer : Consumer<Customer>
    {
        public override async Task ConsumeAsync(ConsumeContext context, Customer message)
        {
            Console.WriteLine(message);
        }
    }
}
