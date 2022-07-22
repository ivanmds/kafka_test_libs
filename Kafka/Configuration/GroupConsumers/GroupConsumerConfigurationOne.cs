namespace Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationOne : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationOne(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationOne Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationOne(listenerConfiguration);
        }
    }
}
