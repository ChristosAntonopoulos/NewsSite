using MongoDB.Driver;
using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Data
{
    public interface IApplicationDbContext
    {
        IMongoCollection<ProcessedArticle> ProcessedArticles { get; }
        IMongoCollection<NewsPipeline> Pipelines { get; }
        IMongoCollection<PipelineExecution> PipelineExecutions { get; }
        Task CreateIndexesAsync(CancellationToken cancellationToken = default);
        Task<PipelineModel?> GetPipelineAsync(string pipelineId);
    }
} 