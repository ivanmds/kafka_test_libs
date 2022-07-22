namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationThree : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationThree(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationThree Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationThree(listenerConfiguration);
        }
    }
}
