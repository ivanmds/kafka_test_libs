
using System;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Metrics;
using Bankly.Sdk.Kafka.Traces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal class KafkaConsumerString : KafkaConsumer<string>
    {
        public KafkaConsumerString(
            IServiceProvider provider,
            ListenerConfiguration listenerConfiguration,
            IProducerMessage producerMessage,
            ILogger logger,
            IMetricService metricService,
            ITraceService traceService,
            IHostApplicationLifetime hostApplicationLifetime)
            : base(provider, listenerConfiguration, producerMessage, logger, metricService, traceService, hostApplicationLifetime)
        {
        }
    }
}
