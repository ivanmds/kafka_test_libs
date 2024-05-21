namespace Bankly.Sdk.Kafka.DefaultValues
{
    internal static class ConstValues
    {
        public const string CONSUMER_GROUP_ID = "consumer_group_id";
        public const string EXCEPTION_FULL_NAME = "exception_full_name";
        public const string TOPIC_NAME = "kafka_topic_name";
        public const string MESSAGE_KEY = "kafka_consume_message_key";
        public const string CORRELATION_ID = "x-correlation-id";
        public const string COMPANY_KEY = "x-company-key";
        public const string RETRY_ATTEMPT = "attempt";
        public const string WILL_RETRY_MESSAGE = "will_retry_message";
        public const string RETRY_MESSAGE = "is_retry_message";
        public const string MESSAGE_STATUS = "message_status";
        public const string MESSAGE_IS_NOTIFICATION = "is_notification";
        public const string IS_INTERNAL_PROCESS = "is_internal_process";
        public const string CONSUMER_NAME = "consumer_name";

        public static class ConsumerMessageStatus
        {
            public const string Error = "ERROR";
            public const string Skipped = "SKIPPED";
            public const string Success = "SUCCESS";
            public const string WillRetry = "WILL_RETRY";
        }

        public static class ProducerMessageStatus
        {
            public const string Error = "ERROR";
            public const string Success = "SUCCESS";
            public const string Unsuccess = "UNSUCCESS";
        }
    }
}
