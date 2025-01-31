using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.Interfaces;

namespace NewsSite.Server.Services.Interfaces
{
    public class PipelineService : IPipelineService
    {
        private readonly IMongoDbConnectionService _connectionService;
        private readonly ILogger<PipelineService> _logger;
        private IMongoCollection<PipelineModel> _pipelines;

        public PipelineService(
            IMongoDbConnectionService connectionService,
            ILogger<PipelineService> logger)
        {
            _connectionService = connectionService;
            _logger = logger;
        }

        private async Task<IMongoCollection<PipelineModel>> GetPipelinesCollectionAsync()
        {
            if (_pipelines == null)
            {
                var database = await _connectionService.GetDatabaseAsync();
                _pipelines = database.GetCollection<PipelineModel>("Pipelines");
            }
            return _pipelines;
        }

        public async Task<IEnumerable<PipelineModel>> GetPipelinesAsync(bool includeDisabled)
        {
            try
            {
                var collection = await GetPipelinesCollectionAsync();
                var filter = includeDisabled ? 
                    Builders<PipelineModel>.Filter.Empty : 
                    Builders<PipelineModel>.Filter.Eq(x => x.IsEnabled, true);
                return await collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pipelines");
                throw;
            }
        }

        public async Task<PipelineModel> GetPipelineByIdAsync(string id)
        {
            try
            {
                var collection = await GetPipelinesCollectionAsync();
                return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pipeline {PipelineId}", id);
                throw;
            }
        }

        public async Task<PipelineModel> CreatePipelineAsync(PipelineModel pipeline)
        {
            try
            {
                var collection = await GetPipelinesCollectionAsync();
                await collection.InsertOneAsync(pipeline);
                return pipeline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pipeline");
                throw;
            }
        }

        public async Task<bool> UpdatePipelineAsync(PipelineModel pipeline)
        {
            try
            {
                var collection = await GetPipelinesCollectionAsync();
                var result = await collection.ReplaceOneAsync(x => x.Id == pipeline.Id, pipeline);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pipeline {PipelineId}", pipeline.Id);
                throw;
            }
        }

        public async Task<bool> DeletePipelineAsync(string id)
        {
            try
            {
                var collection = await GetPipelinesCollectionAsync();
                var result = await collection.DeleteOneAsync(x => x.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pipeline {PipelineId}", id);
                throw;
            }
        }

        public async Task<bool> TogglePipelineStateAsync(string id)
        {
            try
            {
                var pipeline = await GetPipelineByIdAsync(id);
                if (pipeline == null) return false;

                pipeline.IsEnabled = !pipeline.IsEnabled;
                return await UpdatePipelineAsync(pipeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling pipeline state {PipelineId}", id);
                throw;
            }
        }

        public async Task<bool> ReorderPipelineStepsAsync(string id, List<string> stepIds)
        {
            try
            {
                var pipeline = await GetPipelineByIdAsync(id);
                if (pipeline == null) return false;

                var steps = pipeline.Steps.ToDictionary(s => s.Id);
                pipeline.Steps.Clear();

                for (int i = 0; i < stepIds.Count; i++)
                {
                    if (steps.TryGetValue(stepIds[i], out var step))
                    {
                        step.Order = i;
                        pipeline.Steps.Add(step);
                    }
                }

                return await UpdatePipelineAsync(pipeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering pipeline steps {PipelineId}", id);
                throw;
            }
        }
    }
} 