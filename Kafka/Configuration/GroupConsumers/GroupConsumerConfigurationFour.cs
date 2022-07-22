namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationFour : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationFour(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationFour Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationFour(listenerConfiguration);
        }
    }
}
