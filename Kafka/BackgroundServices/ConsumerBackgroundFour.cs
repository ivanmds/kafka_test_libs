using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundFour : ConsumerBackgroundBase<GroupConsumerConfigurationFour>
    {
        public ConsumerBackgroundFour(IServiceProvider services, GroupConsumerConfigurationFour consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
