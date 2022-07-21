﻿using System.Threading.Tasks;

namespace Kafka.Consumers
{
    internal interface IConsumer<TMessage> where TMessage : class
    {
        Task ConsumeAsync(TMessage message);
    }
}