using System;
using Bankly.Sdk.Kafka.Configuration.GroupConsumers;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class ConsumerBackgroundThree : ConsumerBackgroundBase<GroupConsumerConfigurationThree>
    {
        public ConsumerBackgroundThree(IServiceProvider services, GroupConsumerConfigurationThree consumerConfiguration, IProducerMessage producerMessage) : base(services, consumerConfiguration, producerMessage)
        {
        }
    }
}
