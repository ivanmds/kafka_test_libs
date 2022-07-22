using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundSeven : ConsumerBackgroundBase<GroupConsumerConfigurationSeven>
    {
        public ConsumerBackgroundSeven(IServiceProvider services, GroupConsumerConfigurationSeven consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
