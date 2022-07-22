using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundSeven : ConsumerBackgroundBase<GroupConsumerConfigurationSeven>
    {
        public ConsumerBackgroundSeven(IServiceProvider services, GroupConsumerConfigurationSeven consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
