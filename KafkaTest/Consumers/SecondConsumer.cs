using Kafka;

namespace KafkaTest.Consumers
{
    public class SecondConsumer : IConsumer<string>
    {
        public void Consume(string message)
        {
            Console.WriteLine("second consumer" + message);
        }
    }
}
