using Polly;
using Polly.Retry;

namespace NewsSite.Server.Services
{
    public  class RetryPolicy
    {
        public int MaxRetries { get; set; } = 3;
        public int DelaySeconds { get; set; } = 1;
        public  AsyncRetryPolicy CreateDefaultPolicy(ILogger logger)
        {
            return Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(
                            exception,
                            "Retry {RetryCount} after {Delay}s delay due to {Message}",
                            retryCount,
                            timeSpan.TotalSeconds,
                            exception.Message);
                    }
                );
        }
    }
} 