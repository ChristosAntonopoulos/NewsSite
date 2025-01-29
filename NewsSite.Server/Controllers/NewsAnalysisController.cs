//using Microsoft.AspNetCore.Mvc;
//using NewsSite.Server.Models;
//using Microsoft.EntityFrameworkCore;
//using NewsSite.Server.Data;
//using NewsSite.Server.Models.ArticleAggregate;
//using NewsSite.Server.Services.Caching;
//using NewsSite.Server.Services.Pipelines;

//namespace NewsSite.Server.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class NewsAnalysisController : BaseApiController
//    {
//        private readonly ApplicationDbContext _context;

//        public NewsAnalysisController(
//            INewsArticleFetcher articleFetcher,
//            ApplicationDbContext context,
//            ILogger<NewsAnalysisController> logger)
//            : base(logger)
//        {
//            _articleFetcher = articleFetcher;
//            _context = context;
//        }

//        [HttpGet("topics/{topic}/articles")]
//        public async Task<ActionResult<ApiResponse<List<Article>>>> GetArticlesByTopic(
//            string topic, [FromQuery] int count = 10)
//        {
//            try
//            {
//                var articles = await _articleFetcher.FetchArticlesByTopicAsync(topic, count);
//                return OkResponse(articles);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error fetching articles for topic {Topic}", topic);
//                return ErrorResponse<List<Article>>("Error fetching articles");
//            }
//        }

//        [HttpGet("topics/{topic}/trends")]
//        public async Task<ActionResult<TopicTrends>> GetTopicTrends(
//            string topic, 
//            [FromQuery] int days = 7)
//        {
//            try
//            {
//                TopicTrends trends = null;
//                return Ok(trends);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error analyzing trends for topic {Topic}", topic);
//                return StatusCode(500, "Error analyzing trends");
//            }
//        }

//        [HttpGet("emerging-topics")]
//        public async Task<ActionResult<List<EmergingTopic>>> GetEmergingTopics()
//        {
//            try
//            {
//                TopicTrends topics = null;
//                return Ok(topics);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error identifying emerging topics");
//                return StatusCode(500, "Error identifying emerging topics");
//            }
//        }

//    }
//} 