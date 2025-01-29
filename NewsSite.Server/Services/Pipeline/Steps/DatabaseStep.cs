using System.Text.Json;
using Microsoft.Extensions.Logging;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Models.Pipeline;
using NewsSite.Server.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class DatabaseStep : BaseStep
    {
        private readonly IDatabaseOperations _dbOperations;
        private readonly IMongoDbConnectionService _mongoDb;

        public override string StepType => "database.operation";

        public DatabaseStep(
            IDatabaseOperations dbOperations, 
            IMongoDbConnectionService mongoDb,
            ILogger<DatabaseStep> logger) : base(logger)
        {
            _dbOperations = dbOperations;
            _mongoDb = mongoDb;
        }

        private async Task<object> ExecuteQueryAsync(string collection, string query)
        {
            var filter = JsonSerializer.Deserialize<Dictionary<string, object>>(query);
            return await _dbOperations.ExecuteQueryAsync(query);
        }

        private async Task<object> ExecuteInsertAsync(string collection, Dictionary<string, object> data)
        {
            return await _dbOperations.ExecuteInsertAsync(collection, data);
        }

        private async Task<object> ExecuteUpdateAsync(string collection, string query, Dictionary<string, object> data)
        {
            //var filter = JsonSerializer.Deserialize<Dictionary<string, object>>(query);
            return await _dbOperations.ExecuteUpdateAsync(collection, null, data);
        }

        private async Task<object> ExecuteDeleteAsync(string collection, string query)
        {
            var filter = JsonSerializer.Deserialize<Dictionary<string, object>>(query);
            return await _dbOperations.ExecuteDeleteAsync(collection, null);
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters)
        {
            var operation = parameters.GetValueOrDefault("operation", "query");
            var collection = parameters.GetValueOrDefault("collection", "articles");
            var query = parameters.GetValueOrDefault("query", "{}");

            try
            {
                var result = operation switch
                {
                    "query" => await ExecuteQueryAsync(collection, query),
                    "insert" => await ExecuteInsertAsync(collection, input),
                    "update" => await ExecuteUpdateAsync(collection, query, input),
                    "delete" => await ExecuteDeleteAsync(collection, query),
                    _ => throw new ArgumentException($"Unsupported operation: {operation}")
                };

                return ApplyTransformations(new Dictionary<string, object>
                {
                    { "result", result }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing database operation: {Operation}", operation);
                throw;
            }
        }

        public override async Task<bool> ValidateAsync(Dictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("operation") || !parameters.ContainsKey("collection"))
                return false;

            // Validate collection exists
            if (parameters.TryGetValue("collection", out var collection))
            {
                var db = await _mongoDb.GetDatabaseAsync();
                var collections = await db.ListCollectionNamesAsync().Result.ToListAsync();
                if (!collections.Contains(collection))
                {
                    return false;
                }
            }

            // Validate schema mapping if provided
            if (parameters.TryGetValue("schemaMapping", out var schemaMappingJson))
            {
                try
                {
                    JsonSerializer.Deserialize<Dictionary<string, string>>(schemaMappingJson);
                }
                catch
                {
                    return false;
                }
            }

            return IsValidOperation(parameters["operation"]);
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "operation", "collection" };
        }

        private bool IsValidOperation(string operation)
        {
            var validOperations = new[] { "query", "insert", "update", "delete" };
            return validOperations.Contains(operation.ToLower());
        }

        private Dictionary<string, object> MapDataToSchema(IStepData input, Dictionary<string, string> schemaMapping)
        {
            if (schemaMapping == null || !schemaMapping.Any())
                return input.GetData();

            var mappedData = new Dictionary<string, object>();
            foreach (var mapping in schemaMapping)
            {
                var value = input.GetValue<object>(mapping.Key);
                if (value != null)
                {
                    mappedData[mapping.Value] = value;
                }
            }

            return mappedData;
        }

        public async Task<List<string>> GetAvailableCollectionsAsync()
        {
            var db = await _mongoDb.GetDatabaseAsync();
            return await db.ListCollectionNames().ToListAsync();
        }

        public async Task<BsonDocument> GetCollectionSchemaAsync(string collection)
        {
            var db = await _mongoDb.GetDatabaseAsync();
            var coll = db.GetCollection<BsonDocument>(collection);
            
            // Get a sample document to infer schema
            var sample = await coll.Find(new BsonDocument())
                .Limit(1)
                .FirstOrDefaultAsync();

            return sample ?? new BsonDocument();
        }
    }
} 