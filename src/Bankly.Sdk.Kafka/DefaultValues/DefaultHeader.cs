namespace Bankly.Sdk.Kafka.DefaultValues
{
    internal class DefaultHeader
    {
        public const string KeyCorrelationId = "x-correlation-id";
        public const string KeyCompanyKey = "x-company-key";
        public const string KeyIsNotification = "notification-message";
        public const string KeyIsNewClient = "new-client-message";
        public const string KeyResponseToTopic = "response-to-topic";
        public const string KeyEventName = "event-notification-name";
        public const string KeyCommandName = "command-name";
        public const string KeyDefaultMessageName = "default";
        public const string KeyResponseTopic = "response-topic";
        public const string KeyRetryAt = "retry-at";
        public const string KeyCurrentAttempt = "current-attempt";
        public const string KeyWillRetry = "will-retry";
        public const string KeyTraceId = "current-trace-id";
        public const string KeyInternalProcess = "is_internal_process";
        public const string KeyCurrentTopicName = "current-topic-name";
        public const string KeyCurrentGroupId = "current-group-id";
        public const string KeySourceTopicName = "source-topic-name";
    }
}
