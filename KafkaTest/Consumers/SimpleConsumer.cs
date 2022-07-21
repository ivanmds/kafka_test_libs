using Kafka.Consumers;
using KafkaTest.Models;

namespace KafkaTest.Consumers
{
    public class SimpleConsumer : Consumer<Customer>
    {
        public override void BeforeConsume(Customer message)
        {
            Console.WriteLine("before simple consumer " + message.Name);
        }

        public override void Consume(Customer message)
        {
            Console.WriteLine("simple consumer " + message.Name);
        }

        public override void AfterConsume(Customer message)
        {
            Console.WriteLine("after simple consumer " + message.Name);
        }

        public override void ErrorConsume(Exception ex)
        {
            Console.WriteLine("error: " + ex.Message);
        }
    }
}
