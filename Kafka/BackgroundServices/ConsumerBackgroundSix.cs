using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundSix : ConsumerBackgroundBase<GroupConsumerConfigurationSix>
    {
        public ConsumerBackgroundSix(IServiceProvider services, GroupConsumerConfigurationSix consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
