using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Bson;

namespace NewsSite.Server.Data
{
    public class DatabaseOperations : IDatabaseOperations
    {
        private readonly IMongoDbConnectionService _connectionService;
        private readonly ILogger<DatabaseOperations> _logger;

        public DatabaseOperations(IMongoDbConnectionService connectionService, ILogger<DatabaseOperations> logger)
        {
            _connectionService = connectionService;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> ExecuteQueryAsync(string query)
        {
            try
            {
                var database = await _connectionService.GetDatabaseAsync();
                var result = await database.RunCommandAsync<BsonDocument>(query);
                var json = result.ToJson();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", query);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> ExecuteInsertAsync(string collection, Dictionary<string, object> data)
        {
            try
            {
                var database = await _connectionService.GetDatabaseAsync();
                var coll = database.GetCollection<BsonDocument>(collection);
                var json = JsonSerializer.Serialize(data);
                var doc = BsonDocument.Parse(json);
                await coll.InsertOneAsync(doc);
                return new Dictionary<string, object> 
                { 
                    { "_id", doc["_id"].ToString() },
                    { "success", true }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting document into collection: {Collection}", collection);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> ExecuteUpdateAsync(string collection, string filter, Dictionary<string, object> data)
        {
            try
            {
                var database = await _connectionService.GetDatabaseAsync();
                var coll = database.GetCollection<BsonDocument>(collection);
                var filterDoc = BsonDocument.Parse(filter);
                var json = JsonSerializer.Serialize(data);
                var updateDoc = BsonDocument.Parse(json);
                var result = await coll.ReplaceOneAsync(filterDoc, updateDoc);
                return new Dictionary<string, object>
                {
                    { "modifiedCount", result.ModifiedCount },
                    { "success", result.IsAcknowledged }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document in collection: {Collection}", collection);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> ExecuteDeleteAsync(string collection, string filter)
        {
            try
            {
                var database = await _connectionService.GetDatabaseAsync();
                var coll = database.GetCollection<BsonDocument>(collection);
                var filterDoc = BsonDocument.Parse(filter);
                var result = await coll.DeleteOneAsync(filterDoc);
                return new Dictionary<string, object>
                {
                    { "deletedCount", result.DeletedCount },
                    { "success", result.IsAcknowledged }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document from collection: {Collection}", collection);
                throw;
            }
        }
    }
} 