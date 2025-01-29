namespace NewsSite.Server.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await LogRequest(context);
                await _next(context);
            }
            finally
            {
                await LogResponse(context);
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} {RequestBody}",
                context.Request.Method,
                context.Request.Path,
                requestBody);
        }

        private Task LogResponse(HttpContext context)
        {
            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);

            return Task.CompletedTask;
        }
    }
} 