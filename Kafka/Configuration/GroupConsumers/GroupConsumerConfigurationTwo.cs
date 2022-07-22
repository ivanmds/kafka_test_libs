namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationTwo : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationTwo(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationTwo Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationTwo(listenerConfiguration);
        }
    }
}
