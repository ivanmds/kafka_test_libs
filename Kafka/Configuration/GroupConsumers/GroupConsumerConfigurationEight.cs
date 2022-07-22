namespace Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationEight : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationEight(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationEight Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationEight(listenerConfiguration);
        }
    }
}
