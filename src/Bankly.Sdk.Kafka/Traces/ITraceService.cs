using System.Diagnostics;

namespace Bankly.Sdk.Kafka.Traces
{
    internal interface ITraceService
    {
        Activity? StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal, string externalTraceId = null);
    }
}
