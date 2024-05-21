using System.Text.Json;

namespace Bankly.Sdk.Kafka
{
    internal class InternalLogMessage
    {
        public string Message { get; }
        public string TopicName { get; }
        public int Partition { get; }
        public long Offset { get; }
        public string CorrelationId { get; }

        public InternalLogMessage(string message, string topicName, int partition, long offset, string correlationId)
        {
            Message = message;
            TopicName = topicName;
            Partition = partition;
            Offset = offset;
            CorrelationId = correlationId;
        }

        public string ToJson()
            => JsonSerializer.Serialize(this);

        public static InternalLogMessage Create(string message, string topicName, int partition, long offset, string correlationId)
            => new InternalLogMessage(message, topicName, partition, offset, correlationId);
    }
}
