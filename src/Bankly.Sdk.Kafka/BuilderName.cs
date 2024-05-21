using Bankly.Sdk.Contracts.Enums;

namespace Bankly.Sdk.Kafka
{
    public static class BuilderName
    {
        public static string GetTopicName(bool isExternalEvent, Context context, string domainName, string? entityName = null)
        {
            var prefix = isExternalEvent ? "bankly.event" : "private.bankly";
            var topicName = $"{prefix}.{context}.{domainName}";
            if(entityName != null)
                topicName = $"{topicName}.{entityName}";

            return topicName.ToLower();
        }

        public static string GetTopicNameRPC(bool isRequest, Context context, string domainName, string processName)
        {
            var suffix = isRequest ? "request" : "response";
            return $"bankly.{context}.{domainName}.{processName}.{suffix}".ToLower();
        }

        public static string GetGroupIdName(string applicationName, string processName)
           => $"{applicationName}_{processName}".ToLower();

        internal static string GetTopicNameRetry(string currentTopic, string groupId, int secondsToRetry)
            => $"retry_{secondsToRetry}s.{groupId}.{currentTopic}".ToLower();

        internal static string GetTopicNameSkipped(string groupId, string currentTopicName)
            => $"skipped.{groupId}.{currentTopicName}".ToLower();

        internal static string GetTopicNameDeadLetter(string groupId, string currentTopicName)
            => $"dlq.{groupId}.{currentTopicName}".ToLower();
    }
}
