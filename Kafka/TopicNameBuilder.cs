namespace Bankly.Sdk.Kafka
{
    public class TopicNameBuilder
    {
        internal static string GetRetryTopicName(string currentTopic, string groupId, int timeMinutes)
            => $"retry_{timeMinutes}s.{groupId}.{currentTopic}";

    }
}
