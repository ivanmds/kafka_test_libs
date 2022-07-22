namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationSix : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationSix(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationSix Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationSix(listenerConfiguration);
        }
    }
}
