using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundTen : ConsumerBackgroundBase<GroupConsumerConfigurationTen>
    {
        public ConsumerBackgroundTen(IServiceProvider services, GroupConsumerConfigurationTen consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
