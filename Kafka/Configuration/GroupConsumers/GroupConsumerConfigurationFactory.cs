using System;

namespace Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationFactory
    {
        public static GroupConsumerConfigurationBase Factory(ListenerConfiguration listenerConfiguration, int numGroupConsumer)
        {
            switch(numGroupConsumer)
            {
                case 1: return GroupConsumerConfigurationOne.Create(listenerConfiguration);
                default: throw new ArgumentException("Exceeded total group consumer by application");
            }
        }
    }
}
