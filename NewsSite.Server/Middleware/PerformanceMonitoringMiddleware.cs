namespace NewsSite.Server.Middleware
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next,
            IMetricsService metricsService,
            ILogger<PerformanceMonitoringMiddleware> logger)
        {
            _next = next;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {
               

            }
        }
    }
} 