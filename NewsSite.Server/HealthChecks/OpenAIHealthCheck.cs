using Microsoft.Extensions.Diagnostics.HealthChecks;
using NewsSite.Server.Services;

public class OpenAIHealthCheck : IHealthCheck
{
    //private readonly IOpenAIService _openAIService;

    public OpenAIHealthCheck()
    {
        //_openAIService = openAIService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple test analysis
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("OpenAI service is not responding", ex);
        }
    }
} 