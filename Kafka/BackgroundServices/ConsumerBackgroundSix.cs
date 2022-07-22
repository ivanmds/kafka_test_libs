using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundSix : ConsumerBackgroundBase<GroupConsumerConfigurationSix>
    {
        public ConsumerBackgroundSix(IServiceProvider services, GroupConsumerConfigurationSix consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
