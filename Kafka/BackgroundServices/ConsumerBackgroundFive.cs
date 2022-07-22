using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundFive : ConsumerBackgroundBase<GroupConsumerConfigurationFive>
    {
        public ConsumerBackgroundFive(IServiceProvider services, GroupConsumerConfigurationFive consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
