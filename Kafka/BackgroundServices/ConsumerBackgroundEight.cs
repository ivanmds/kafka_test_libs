using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundEight : ConsumerBackgroundBase<GroupConsumerConfigurationEight>
    {
        public ConsumerBackgroundEight(IServiceProvider services, GroupConsumerConfigurationEight consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
