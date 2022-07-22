using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundTwo : ConsumerBackgroundBase<GroupConsumerConfigurationTwo>
    {
        public ConsumerBackgroundTwo(IServiceProvider services, GroupConsumerConfigurationTwo consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
