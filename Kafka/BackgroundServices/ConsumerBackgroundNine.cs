using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundNine : ConsumerBackgroundBase<GroupConsumerConfigurationNine>
    {
        public ConsumerBackgroundNine(IServiceProvider services, GroupConsumerConfigurationNine consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
