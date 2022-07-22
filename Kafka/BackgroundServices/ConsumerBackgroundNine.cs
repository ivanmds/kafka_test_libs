using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundNine : ConsumerBackgroundBase<GroupConsumerConfigurationNine>
    {
        public ConsumerBackgroundNine(IServiceProvider services, GroupConsumerConfigurationNine consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
