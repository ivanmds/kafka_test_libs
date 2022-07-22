using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundFive : ConsumerBackgroundBase<GroupConsumerConfigurationFive>
    {
        public ConsumerBackgroundFive(IServiceProvider services, GroupConsumerConfigurationFive consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
