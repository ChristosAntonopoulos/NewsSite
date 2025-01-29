//using Microsoft.EntityFrameworkCore;
//using NewsSite.Server.Data;

//namespace NewsSite.Server.Services.Caching
//{
//    public class CleanupService : BackgroundService
//    {
//        private readonly IServiceProvider _services;
//        private readonly ILogger<CleanupService> _logger;
//        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

//        public CleanupService(IServiceProvider services, ILogger<CleanupService> logger)
//        {
//            _services = services;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    await CleanupOldData();
//                    await Task.Delay(_interval, stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error during cleanup");
//                }
//            }
//        }

//        private async Task CleanupOldData()
//        {
//            using var scope = _services.CreateScope();
//            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//            var cutoffDate = DateTime.UtcNow.AddDays(-30);

//            // Cleanup old processing logs
//            var oldLogs = await context.ProcessingLogs
//                .Where(l => l.CreatedAt < cutoffDate)
//                .ToListAsync();

//            context.ProcessingLogs.RemoveRange(oldLogs);

//            // Cleanup old pipeline executions
//            var oldExecutions = await context.PipelineExecutions
//                .Where(e => e.CompletedAt < cutoffDate)
//                .ToListAsync();

//            context.PipelineExecutions.RemoveRange(oldExecutions);

//            await context.SaveChangesAsync();

//            _logger.LogInformation(
//                "Cleaned up {LogCount} logs and {ExecutionCount} executions",
//                oldLogs.Count,
//                oldExecutions.Count);
//        }
//    }
//}