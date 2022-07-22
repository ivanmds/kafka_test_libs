using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundTwo : ConsumerBackgroundBase<GroupConsumerConfigurationTwo>
    {
        public ConsumerBackgroundTwo(IServiceProvider services, GroupConsumerConfigurationTwo consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
