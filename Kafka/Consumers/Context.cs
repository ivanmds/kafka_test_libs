using Kafka.Values;

namespace Kafka.Consumers
{
    public class Context
    {
        internal Context(HeaderValue header)
        { 
            Header = header;
            IsNotification = header.IsNotification();
            AnswersTopic = header.GetAnswersTopic();
            CorrelationId = header.GetCorrelationId();
        }

        public HeaderValue Header { get; private set; }

        public static Context Create(HeaderValue header)
            => new Context(header);

        public bool IsNotification { get; private set; }
        public string AnswersTopic { get; private set; }
        public string CorrelationId { get; private set; }
    }
}
