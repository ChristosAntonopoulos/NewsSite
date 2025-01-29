using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace NewsSite.Server.Monitoring
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = "NewsSite.Server";
            telemetry.Context.Component.Version = "1.0.0";
        }
    }
} 