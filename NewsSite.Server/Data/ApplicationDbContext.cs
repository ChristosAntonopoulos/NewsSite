using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using NewsSite.Server.Models.PipelineAggregate;
using System.Collections.Concurrent;

namespace NewsSite.Server.Data
{ 
    public class ApplicationDbContext : IApplicationDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;
        private readonly ConcurrentDictionary<string, PipelineModel> _pipelines = new();

        public ApplicationDbContext(IOptions<MongoDbSettings> settings)
        {
            _settings = settings.Value;
            
            var mongoSettings = MongoClientSettings.FromConnectionString(_settings.ConnectionString);
            mongoSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
            mongoSettings.RetryWrites = true;
            mongoSettings.RetryReads = true;
            
            var client = new MongoClient(mongoSettings);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<ProcessedArticle> ProcessedArticles => 
            _database.GetCollection<ProcessedArticle>(_settings.CollectionNames.ProcessedArticles);

        public IMongoCollection<NewsPipeline> Pipelines =>
            _database.GetCollection<NewsPipeline>(_settings.CollectionNames.Pipelines);

      

        public IMongoCollection<PipelineExecution> PipelineExecutions =>
            _database.GetCollection<PipelineExecution>(_settings.CollectionNames.PipelineExecutions);

        public async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
        {
            // ProcessedArticles indexes
            var processedArticlesBuilder = Builders<ProcessedArticle>.IndexKeys;
            var processedArticlesIndexes = new[]
            {
                new CreateIndexModel<ProcessedArticle>(
                    processedArticlesBuilder.Ascending(x => x.PipelineId)),
                new CreateIndexModel<ProcessedArticle>(
                    processedArticlesBuilder.Ascending(x => x.PublishedAt))
            };
            await ProcessedArticles.Indexes.CreateManyAsync(processedArticlesIndexes, cancellationToken);

            // Pipelines indexes
            var pipelinesBuilder = Builders<NewsPipeline>.IndexKeys;
            var pipelinesIndexes = new[]
            {
                new CreateIndexModel<NewsPipeline>(
                    pipelinesBuilder.Descending(x => x.CreatedAt),
                    new CreateIndexOptions { Unique = true })
            };
            await Pipelines.Indexes.CreateManyAsync(pipelinesIndexes, cancellationToken);

            // ProcessingLogs indexes
          

            // PipelineExecutions indexes
            var executionsBuilder = Builders<PipelineExecution>.IndexKeys;
            var executionsIndexes = new[]
            {
                new CreateIndexModel<PipelineExecution>(
                    executionsBuilder.Ascending(x => x.StartedAt)),
                new CreateIndexModel<PipelineExecution>(
                    executionsBuilder.Ascending(x => x.PipelineId))
            };
            await PipelineExecutions.Indexes.CreateManyAsync(executionsIndexes, cancellationToken);
        }

        public async Task<PipelineModel?> GetPipelineAsync(string pipelineId)
        {
            _pipelines.TryGetValue(pipelineId, out var pipeline);
            return await Task.FromResult(pipeline);
        }
    }
}