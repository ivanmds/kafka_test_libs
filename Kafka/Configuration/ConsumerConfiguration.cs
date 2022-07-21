using System;

namespace Kafka.Configuration
{
    public class ConsumerConfiguration<TConsumer>
        where TConsumer : class
    {
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly Type _typeMessage;
        private readonly Type _typeConsumer;
        private readonly string? _eventName;

        public ConsumerConfiguration(ListenerConfiguration listenerConfiguration, Type typeMessage, Type typeConsumer, string? eventName = null)
        {
            _listenerConfiguration = listenerConfiguration;
            _typeMessage = typeMessage;
            _typeConsumer = typeConsumer;
            _eventName = eventName;
        }

        public static ConsumerConfiguration<TConsumer> Create<TMessage>(ListenerConfiguration listenerConfiguration, string? eventName = null)
            where TMessage : class
        {
            var messageType = typeof(TMessage);
            var consumerType = typeof(TConsumer);

            return new ConsumerConfiguration<TConsumer>(listenerConfiguration, messageType, consumerType, eventName);
        }

        public ListenerConfiguration ListenerConfiguration => _listenerConfiguration;
        public Type TypeMessage => _typeMessage;
        public Type TypeConsumer => _typeConsumer;
        public string? EventName => _eventName;
    }
}
