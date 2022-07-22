namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationFive : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationFive(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationFive Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationFive(listenerConfiguration);
        }
    }
}
