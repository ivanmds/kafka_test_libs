using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka.Metrics
{
    internal class MetricService : IMetricService
    {
        private static Meter? METER;
        private static Histogram<int> KAFKA_CONSUMER_ELAPSED_TIME;
        private static Histogram<int> KAFKA_PRODUCER_ELAPSED_TIME;
        private static bool _hasStarted = false;


        public MetricService(IServiceProvider provider)
        {
            METER = provider.GetService(typeof(Meter)) as Meter;

            if(METER != null)
            {
                KAFKA_CONSUMER_ELAPSED_TIME = METER.CreateHistogram<int>("bankly_kafka_consumer_elapsed_time");
                KAFKA_PRODUCER_ELAPSED_TIME = METER.CreateHistogram<int>("bankly_kafka_producer_elapsed_time");
                _hasStarted = true;
            }
        }

        public static KeyValuePair<string, object?> CreateTag(string name, string value)
            => new KeyValuePair<string, object?>(name, value);

        public KeyValuePair<string, object?> CreateCustomTag(string name, string value)
            => CreateTag(name, value);

        public void RecordConsumerElapsedTime(int elapsed, params KeyValuePair<string, object?>[] tags)
        {
            if(_hasStarted && KafkaTelemetric.MetricLevel > TelemetricLevel.Low)
            {
                KAFKA_CONSUMER_ELAPSED_TIME.Record(elapsed, tags);
            }
        }

        public void RecordProducerElapsedTime(int elapsed, params KeyValuePair<string, object?>[] tags)
        {
            if(_hasStarted && KafkaTelemetric.MetricLevel > TelemetricLevel.Medium)
            {
                KAFKA_PRODUCER_ELAPSED_TIME.Record(elapsed, tags);
            }
        }
    }
}