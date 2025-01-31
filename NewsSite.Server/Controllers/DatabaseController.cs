using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NewsSite.Server.Services.Pipeline.Steps;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseStep _databaseStep;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(DatabaseStep databaseStep, ILogger<DatabaseController> logger)
        {
            _databaseStep = databaseStep;
            _logger = logger;
        }

        [HttpGet("collections")]
        public async Task<ActionResult<List<string>>> GetCollections()
        {
            try
            {
                var collections = await _databaseStep.GetAvailableCollectionsAsync();
                return Ok(collections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve collections");
                return StatusCode(500, "Failed to retrieve collections");
            }
        }

        [HttpGet("collections/{collection}/schema")]
        public async Task<ActionResult<BsonDocument>> GetCollectionSchema(string collection)
        {
            try
            {
                var schema = await _databaseStep.GetCollectionSchemaAsync(collection);
                return Ok(schema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve schema for collection {Collection}", collection);
                return StatusCode(500, $"Failed to retrieve schema for collection {collection}");
            }
        }
    }
} 