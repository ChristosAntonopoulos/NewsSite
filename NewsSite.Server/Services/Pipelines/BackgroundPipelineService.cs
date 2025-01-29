using System.Collections.Concurrent;

namespace NewsSite.Server.Services.Pipelines
{
    public class BackgroundPipelineService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BackgroundPipelineService> _logger;
        private readonly ConcurrentQueue<int> _pipelineQueue = new();

        public BackgroundPipelineService(IServiceProvider services, ILogger<BackgroundPipelineService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public void QueuePipeline(int pipelineId)
        {
            _pipelineQueue.Enqueue(pipelineId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_pipelineQueue.TryDequeue(out int pipelineId))
                {
                    try
                    {
                        using var scope = _services.CreateScope();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing pipeline {PipelineId}", pipelineId);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}