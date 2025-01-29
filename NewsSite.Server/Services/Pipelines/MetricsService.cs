using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public interface IMetricsService
{
    void TrackPipelineExecution(int pipelineId, TimeSpan duration);
    void TrackArticlesProcessed(int count);
    void TrackApiCallDuration(string endpoint, TimeSpan duration);
}

public class MetricsService  : IMetricsService
{

    public MetricsService()
    {
    }

    public void TrackPipelineExecution(int pipelineId, TimeSpan duration)
    {
       
    }

    public void TrackArticlesProcessed(int count)
    {
    }
    public void RecordStepDuration(string stepName, TimeSpan duration)
    {
          //_logger.LogInformation("Step {StepName} completed in {Duration}ms", stepName, duration.TotalMilliseconds);
        // Add actual metrics recording logic here (e.g., Prometheus, ApplicationInsights)
    }

    public void TrackApiCallDuration(string endpoint, TimeSpan duration)
    {
       
    }
} 