using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundThree : ConsumerBackgroundBase<GroupConsumerConfigurationThree>
    {
        public ConsumerBackgroundThree(IServiceProvider services, GroupConsumerConfigurationThree consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
