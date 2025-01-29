//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using NewsSite.Server.Models;
//using NewsSite.Server.Services;

//namespace NewsSite.Server.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class NewsPipelineController : ControllerBase
//    {
//        private readonly INewsPipelineService _pipelineService;
//        private readonly ILogger<NewsPipelineController> _logger;
//        // TODO: IBackgroundPipelineService needs to be defined in a service interface
//        // and registered in Program.cs. For now, removing unused field.

//        public NewsPipelineController(
//            INewsPipelineService pipelineService,
//            ILogger<NewsPipelineController> logger)
//        {
//            _pipelineService = pipelineService;
//            _logger = logger;
           
//        }

//        [HttpPost]
//        public async Task<ActionResult<NewsPipeline>> CreatePipeline(NewsPipeline pipeline)
//        {
//            try
//            {
//                var result = await _pipelineService.CreatePipelineAsync(pipeline);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating pipeline");
//                return StatusCode(500, "Error creating pipeline");
//            }
//        }

//        [HttpPost("{id}/execute")]
//        public async Task<ActionResult<List<ProcessedArticle>>> ExecutePipeline(int id)
//        {
//            try
//            {
//                var results = await _pipelineService.ExecutePipelineAsync(id);
//                return Ok(results);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error executing pipeline {PipelineId}", id);
//                return StatusCode(500, "Error executing pipeline");
//            }
//        }

//        [HttpGet("{id}/articles")]
//        public async Task<ActionResult<PagedResult<ProcessedArticle>>> GetArticles(
//            int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
//        {
//            try
//            {
//                var result = await _pipelineService.GetArticlesAsync(id, page, pageSize);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving articles for pipeline {PipelineId}", id);
//                return StatusCode(500, "Error retrieving articles");
//            }
//        }

//        [HttpPost("{id}/execute-background")]
//        public ActionResult QueuePipelineExecution(int id)
//        {
//            try
//            {
//               // _backgroundPipelineService.QueuePipeline(id);
//                return Accepted();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error queuing pipeline {PipelineId}", id);
//                return StatusCode(500, "Error queuing pipeline");
//            }
//        }
//    }
//} 