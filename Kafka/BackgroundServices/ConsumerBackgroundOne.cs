using System;
using Kafka.Configuration.GroupConsumers;

namespace Kafka.BackgroundServices
{
    internal class ConsumerBackgroundOne : ConsumerBackgroundBase<GroupConsumerConfigurationOne>
    {
        public ConsumerBackgroundOne(IServiceProvider services, GroupConsumerConfigurationOne consumerConfiguration) 
            : base(services, consumerConfiguration) { }
    }
}
