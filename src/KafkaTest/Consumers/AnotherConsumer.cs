using Bankly.Sdk.Kafka.Consumers;
using KafkaTest.Models;
using Newtonsoft.Json;

namespace KafkaTest.Consumers
{
    public class AnotherConsumer : Consumer<Customer>
    {
        public override async Task ConsumeAsync(ConsumeContext context, Customer message)
        {
            var msg = JsonConvert.SerializeObject(message);
            Console.WriteLine(msg);
        }
    }


    public class ExampleConsumer : Consumer<Customer>
    {
        public override async Task ConsumeAsync(ConsumeContext context, Customer message)
        {
            Console.WriteLine(message);
        }
    }


}
