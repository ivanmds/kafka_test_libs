using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundTen : ConsumerBackgroundBase<GroupConsumerConfigurationTen>
    {
        public ConsumerBackgroundTen(IServiceProvider services, GroupConsumerConfigurationTen consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
