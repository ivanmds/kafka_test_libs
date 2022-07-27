﻿namespace Bankly.Sdk.Kafka.DefaultValues
{
    internal class DefaultHeader
    {
        public const string KeyCorrelationId = "x-correlation-id";
        public const string KeyIsNotification = "notification-message";
        public const string KeyResponseToTopic = "response-to-topic";
        public const string KeyEventName = "event-notification-name";
        public const string KeyDefaultEvenName = "default";
        public const string KeyResponseTopic = "response-topic";
        public const string KeyRetryAt = "retry-at";
        public const string KeyCurrentAttempt = "current-attempt";
        public const string KeyWillRetry = "will-retry";
    }
}
