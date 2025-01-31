using Microsoft.AspNetCore.Mvc;
using NewsSite.Server.Models;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseApiController(ILogger logger)
        {
            _logger = logger;
        }

        protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string message = null)
        {
            return Ok(ApiResponse<T>.Ok(data, message));
        }

        protected ActionResult<ApiResponse<T>> ErrorResponse<T>(string message, int statusCode = 500)
        {
            var response = ApiResponse<T>.Error(message, HttpContext.TraceIdentifier);
            return StatusCode(statusCode, response);
        }
    }
} 