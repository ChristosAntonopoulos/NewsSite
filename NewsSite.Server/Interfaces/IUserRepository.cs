using NewsSite.Server.Models;

namespace NewsSite.Server.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByGoogleIdAsync(string googleId);
        Task<User?> GetUserByRedditIdAsync(string redditId);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UpdateLastLoginAsync(string id);
        Task<bool> UpdatePasswordAsync(string id, string passwordHash);
        Task<bool> LinkGoogleAccountAsync(string id, string googleId);
        Task<bool> LinkRedditAccountAsync(string id, string redditId);
        Task<bool> UpdateLanguageAsync(string id, string language);
        Task<bool> UpdateThemeAsync(string id, string theme);
    }
} 