using System.Collections.Generic;

namespace Bankly.Sdk.Kafka.Metrics
{
    internal interface IMetricService
    {
        void RecordConsumerElapsedTime(int elapsed, params KeyValuePair<string, object?>[] tags);
        void RecordProducerElapsedTime(int elapsed, params KeyValuePair<string, object?>[] tags);
        KeyValuePair<string, object?> CreateCustomTag(string name, string value);
    }
}
