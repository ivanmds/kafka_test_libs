namespace Bankly.Sdk.Kafka.Configuration.GroupConsumers
{ 
    internal abstract class GroupConsumerConfigurationBase
    {
        private readonly ListenerConfiguration _listenerConfiguration;

        public GroupConsumerConfigurationBase(ListenerConfiguration listenerConfiguration)
        {
            _listenerConfiguration = listenerConfiguration;
        }

        public string GetConsumerKey(string eventName) 
            => GetConsumerKey(_listenerConfiguration.GroupId, eventName);

        public string GetEventNameFromConsumerKey(string keyConsumer)
        {
            return keyConsumer.Replace(_listenerConfiguration.GroupId, "");
        }

        public ListenerConfiguration ListenerConfiguration => _listenerConfiguration;

        public static string GetConsumerKey(string groupId, string eventName)
        {
            var sufixName = eventName;

            if (string.IsNullOrWhiteSpace(sufixName))
                sufixName = DefaultValues.DefaultHeader.KeyDefaultEvenName;

            return $"{groupId}#{sufixName}";
        }

    }
}
