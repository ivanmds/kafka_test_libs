namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationSeven : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationSeven(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationSeven Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationSeven(listenerConfiguration);
        }
    }
}
