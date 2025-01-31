//using System.Collections.Concurrent;
//using System.Threading.RateLimiting;

//namespace NewsSite.Server.Middleware
//{
//    public class RateLimitingMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<RateLimitingMiddleware> _logger;
//        private static readonly FixedWindowRateLimiter _rateLimiter = new(new FixedWindowRateLimiterOptions
//        {
//            PermitLimit = 10,
//            Window = TimeSpan.FromSeconds(1)
//        });

//        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            var result = await _rateLimiter(1);
            
//            if (result.IsAcquired)
//            {
//                await _next(context);
//            }
//            else
//            {
//                _logger.LogWarning("Rate limit exceeded for IP: {IP}", context.Connection.RemoteIpAddress);
//                context.Response.StatusCode = 429; // Too Many Requests
//                await context.Response.WriteAsync("Too many requests. Please try again later.");
//            }
//        }
//    }
//} 