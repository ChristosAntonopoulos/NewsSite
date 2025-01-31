using Microsoft.AspNetCore.Mvc;
using NewsSite.Server.Services.News;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsCategoryService _newsCategoryService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            INewsCategoryService newsCategoryService,
            ILogger<NewsController> logger)
        {
            _newsCategoryService = newsCategoryService;
            _logger = logger;
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<NewsSearchResult>> GetNewsByCategory(string category)
        {
            try
            {
                _logger.LogInformation($"Received request for news category: {category}");
                var result = await _newsCategoryService.GetNewsByCategory(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching news for category: {category}");
                return StatusCode(500, "An error occurred while fetching news");
            }
        }
    }
} 