﻿namespace Kafka.DefaultValues
{
    internal class DefaultHeader
    {
        public static string KeyCorrelationId => "x-correlation-id";
        public static string KeyIsNotification => "notification-message";
        public static string KeyResponseToTopic => "response-to-topic";
    }
}
