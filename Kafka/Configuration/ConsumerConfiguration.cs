using System;

namespace Kafka.Configuration
{
    public class ConsumerConfiguration<TConsumer>
        where TConsumer : class
    {
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly Type _typeConsumer;
        private readonly string? _eventName;

        public ConsumerConfiguration(ListenerConfiguration listenerConfiguration, Type typeConsumer, string? eventName = null)
        {
            _listenerConfiguration = listenerConfiguration;
            _typeConsumer = typeConsumer;
            _eventName = eventName;
        }

        public static ConsumerConfiguration<TConsumer> Create(ListenerConfiguration listenerConfiguration, string? eventName = null)
        {
            var consumerType = typeof(TConsumer);

            return new ConsumerConfiguration<TConsumer>(listenerConfiguration, consumerType, eventName);
        }

        public ListenerConfiguration ListenerConfiguration => _listenerConfiguration;
        public Type TypeConsumer => _typeConsumer;
        public string? EventName => _eventName;
    }
}
