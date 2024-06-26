using Bankly.Sdk.Kafka.Values;

namespace Bankly.Sdk.Kafka.Consumers
{
    public class ConsumeContext
    {
        internal ConsumeContext(HeaderValue header)
        {
            IsNotification = header.GetIsNotification();
            ResponseTopic = header.GetResponseTopic();
            CorrelationId = header.GetCorrelationId();
            IsRetry = header.GetCurrentAttempt() > 0;
            WillRetry = header.GetWillRetry();
            Attempt = header.GetCurrentAttempt() == 0 ? null : (int?)header.GetCurrentAttempt();
            TopicName = header.GetCurrentTopicName();
            GroupId = header.GetCurrentGroupId();
            SourceTopicName = header.GetSourceTopicName();
            Header = header;
        }

        public static ConsumeContext Create(HeaderValue header)
            => new ConsumeContext(header);

        public bool IsNotification { get; private set; }
        public string ResponseTopic { get; private set; }
        public string CorrelationId { get; private set; }
        public bool IsRetry { get; private set; }
        public bool WillRetry { get; private set; }
        public int? Attempt { get; private set; }
        public string TopicName { get; private set; }
        public string GroupId { get; private set; }
        public string SourceTopicName { get; private set; }

        public HeaderValue Header { get; private set; }
    }
}
