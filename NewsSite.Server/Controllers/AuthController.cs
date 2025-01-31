using Microsoft.AspNetCore.Mvc;
using NewsSite.Server.Models;
using NewsSite.Server.Models.Auth;
using NewsSite.Server.Services.Interfaces;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
            : base(logger)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return OkResponse(response, "Login successful");
            }
            catch (Exception ex)
            {
                return ErrorResponse<AuthResponse>(ex.Message, StatusCodes.Status401Unauthorized);
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return OkResponse(response, "Registration successful");
            }
            catch (Exception ex)
            {
                return ErrorResponse<AuthResponse>(ex.Message, StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("google")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin([FromBody] string token)
        {
            try
            {
                var response = await _authService.GoogleLoginAsync(token);
                return OkResponse(response, "Google login successful");
            }
            catch (Exception ex)
            {
                return ErrorResponse<AuthResponse>(ex.Message, StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("reddit")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RedditLogin([FromBody] string code)
        {
            try
            {
                var response = await _authService.RedditLoginAsync(code);
                return OkResponse(response, "Reddit login successful");
            }
            catch (Exception ex)
            {
                return ErrorResponse<AuthResponse>(ex.Message, StatusCodes.Status400BadRequest);
            }
        }
    }
} 