using Bankly.Sdk.Kafka.Values;

namespace Bankly.Sdk.Kafka.Consumers
{
    public class Context
    {
        internal Context(HeaderValue header)
        { 
            Header = header;
            IsNotification = header.IsNotification();
            ResponseTopic = header.GetResponseTopic();
            CorrelationId = header.GetCorrelationId();
            IsRetry = header.GetCurrentAttempt() > 0;
            WillRetry = header.GetWillRetry();
        }

        public HeaderValue Header { get; private set; }

        public static Context Create(HeaderValue header)
            => new Context(header);

        public bool IsNotification { get; private set; }
        public string ResponseTopic { get; private set; }
        public string CorrelationId { get; private set; }
        public bool IsRetry { get; private set; }
        public bool WillRetry { get; private set; }
    }
}
