namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{
    internal class GroupConsumerConfigurationTen : GroupConsumerConfigurationBase
    {
        public GroupConsumerConfigurationTen(ListenerConfiguration listenerConfiguration)
            : base(listenerConfiguration) { }

        public static GroupConsumerConfigurationTen Create(ListenerConfiguration listenerConfiguration)
        {
            return new GroupConsumerConfigurationTen(listenerConfiguration);
        }
    }
}
