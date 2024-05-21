using System;
using System.Diagnostics;
using Bankly.Sdk.Kafka.Configuration;

namespace Bankly.Sdk.Kafka.Traces
{
    internal class TraceService : ITraceService
    {
        private static ActivitySource? ACTIVITY_SOURCE;
        private static bool _hasStarted = false;

        public TraceService(IServiceProvider provider)
        {
            ACTIVITY_SOURCE = provider.GetService(typeof(ActivitySource)) as ActivitySource;
            if(ACTIVITY_SOURCE != null)
                _hasStarted = true;
        }

        public Activity? StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal, string externalTraceId = null)
        {
            if(_hasStarted && KafkaTelemetric.TraceLevel > TelemetricLevel.Low)
            {
                if(string.IsNullOrEmpty(externalTraceId))
                    return ACTIVITY_SOURCE.StartActivity(name, kind);
                else
                {
                    var (parentTraceId, parentSpanId) = GetParents(externalTraceId);
                    if(parentTraceId is null || parentSpanId is null)
                        return StartActivity(name, kind);

                    var parentContext = new ActivityContext(
                        ActivityTraceId.CreateFromString(parentTraceId),
                        ActivitySpanId.CreateFromString(parentSpanId),
                        ActivityTraceFlags.Recorded);

                    return ACTIVITY_SOURCE.StartActivity(name, kind, parentContext);
                }
            }


            return null;
        }

        private (string parentTraceId, string parentSpanId) GetParents(string externalTraceId)
        {
            try
            {
                var traceValues = externalTraceId.Split('-');
                if(traceValues.Length > 0)
                {
                    return (traceValues[1], traceValues[2]);
                }

                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
