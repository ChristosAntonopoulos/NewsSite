using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NewsSite.Server.HealthChecks
{
    public class HealthCheckResponse
    {
        public string Status { get; set; }
        public IEnumerable<HealthCheckItem> Checks { get; set; }
        public TimeSpan Duration { get; set; }

        public class HealthCheckItem
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public string Description { get; set; }
            public TimeSpan Duration { get; set; }
        }

        public static Task WriteHealthCheckUIResponse(HttpContext context, HealthReport report)
        {
            var response = new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(e => new HealthCheckItem
                {
                    Name = e.Key,
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description ?? "",
                    Duration = e.Value.Duration
                }),
                Duration = report.TotalDuration
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
} 