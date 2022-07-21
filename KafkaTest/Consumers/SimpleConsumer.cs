using Kafka;

namespace KafkaTest.Consumers
{
    public class SimpleConsumer : IConsumer<string>
    {
        public void Consume(string message)
        {
            Console.WriteLine("simple consumer " + message);
        }
    }
}
