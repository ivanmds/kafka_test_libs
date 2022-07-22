using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundEight : ConsumerBackgroundBase<GroupConsumerConfigurationEight>
    {
        public ConsumerBackgroundEight(IServiceProvider services, GroupConsumerConfigurationEight consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
