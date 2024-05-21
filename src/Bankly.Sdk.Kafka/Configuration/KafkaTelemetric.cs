namespace Bankly.Sdk.Kafka.Configuration
{
    public class KafkaTelemetric
    {
        internal static TelemetricLevel LogLevel { get; set; }
        internal static TelemetricLevel MetricLevel { get; set; }
        internal static TelemetricLevel TraceLevel { get; set; }

        internal KafkaTelemetric(
            TelemetricLevel logLevel,
            TelemetricLevel metricLevel,
            TelemetricLevel traceLevel) {

            LogLevel = logLevel;
            MetricLevel = metricLevel;
            TraceLevel = traceLevel;
        }

        public static KafkaTelemetric Setting(
           TelemetricLevel logLevel = TelemetricLevel.Low,
           TelemetricLevel metricLevel = TelemetricLevel.Low,
           TelemetricLevel traceLevel = TelemetricLevel.Low)
            => new KafkaTelemetric(logLevel, metricLevel, traceLevel);
    }
}
