using NewsSite.Server.Models;
using NewsSite.Server.Models.Auth;

namespace NewsSite.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> GoogleLoginAsync(string token);
        Task<AuthResponse> RedditLoginAsync(string code);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<bool> ValidatePasswordAsync(string password, string passwordHash);
        Task<User> GetUserByIdAsync(string id);
        Task<User> GetUserByEmailAsync(string email);
    }
} 