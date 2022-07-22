namespace Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationNine : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationNine(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationNine Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationNine(listenerConfiguration);
        }
    }
}
