using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundOne : ConsumerBackgroundBase<GroupConsumerConfigurationOne>
    {
        public ConsumerBackgroundOne(IServiceProvider services, GroupConsumerConfigurationOne consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
