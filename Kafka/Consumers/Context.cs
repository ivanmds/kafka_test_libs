using Kafka.Values;

namespace Kafka.Consumers
{
    public class Context
    {
        internal Context(HeaderValue header)
        { 
            Header = header;
        }

        public HeaderValue Header { get; private set; }

        public static Context Create(HeaderValue header)
            => new Context(header);
    }
}
