using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundFour : ConsumerBackgroundBase<GroupConsumerConfigurationFour>
    {
        public ConsumerBackgroundFour(IServiceProvider services, GroupConsumerConfigurationFour consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
