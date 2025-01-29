using Microsoft.AspNetCore.Mvc;
using NewsSite.Server.Services;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingController : ControllerBase
    {
        private readonly ITrendingService _trendingService;

        public TrendingController(ITrendingService trendingService)
        {
            _trendingService = trendingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrending()
        {
            var trends = await _trendingService.GetTrendingTopicsAsync();
            return Ok(trends);
        }
    }
} 