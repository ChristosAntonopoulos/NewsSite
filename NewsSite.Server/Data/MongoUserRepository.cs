using MongoDB.Driver;
using NewsSite.Server.Interfaces;
using NewsSite.Server.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace NewsSite.Server.Data
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoDbConnectionService _connectionService;
        private readonly ILogger<MongoUserRepository> _logger;
        private IMongoCollection<User>? _users;

        public MongoUserRepository(
            IMongoDbConnectionService connectionService,
            ILogger<MongoUserRepository> logger)
        {
            _connectionService = connectionService;
            _logger = logger;
        }

        private async Task<IMongoCollection<User>> GetUsersCollectionAsync()
        {
            try
            {
                if (_users == null)
                {
                    var database = await _connectionService.GetDatabaseAsync();
                    _users = database.GetCollection<User>("Users");

                    // Create unique index for email
                    var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(user => user.Email);
                    var indexOptions = new CreateIndexOptions { Unique = true };
                    var indexModel = new CreateIndexModel<User>(indexKeysDefinition, indexOptions);
                    await _users.Indexes.CreateOneAsync(indexModel);

                    // Create indexes for social logins
                    await _users.Indexes.CreateOneAsync(
                        new CreateIndexModel<User>(
                            Builders<User>.IndexKeys.Ascending(u => u.GoogleId),
                            new CreateIndexOptions { Sparse = true }
                        )
                    );
                    await _users.Indexes.CreateOneAsync(
                        new CreateIndexModel<User>(
                            Builders<User>.IndexKeys.Ascending(u => u.RedditId),
                            new CreateIndexOptions { Sparse = true }
                        )
                    );
                }
                return _users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Users collection");
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                return await collection.Find(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with email {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetUserByGoogleIdAsync(string googleId)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                return await collection.Find(x => x.GoogleId == googleId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with Google ID {GoogleId}", googleId);
                throw;
            }
        }

        public async Task<User?> GetUserByRedditIdAsync(string redditId)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                return await collection.Find(x => x.RedditId == redditId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with Reddit ID {RedditId}", redditId);
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var collection = await GetUsersCollectionAsync();

                // Check if email already exists
                var existingUser = await GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("Email already exists");
                }

                await collection.InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in MongoDB");
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var collection = await GetUsersCollectionAsync();
                var result = await collection.ReplaceOneAsync(x => x.Id == user.Id, user);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", user?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var result = await collection.DeleteOneAsync(x => x.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(string id)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
                var result = await collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UpdatePasswordAsync(string id, string passwordHash)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var update = Builders<User>.Update.Set(u => u.PasswordHash, passwordHash);
                var result = await collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> LinkGoogleAccountAsync(string id, string googleId)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var update = Builders<User>.Update.Set(u => u.GoogleId, googleId);
                var result = await collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Google account for user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> LinkRedditAccountAsync(string id, string redditId)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var update = Builders<User>.Update.Set(u => u.RedditId, redditId);
                var result = await collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Reddit account for user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateLanguageAsync(string id, string language)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var update = Builders<User>.Update.Set(u => u.Language, language);
                var result = await collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating language for user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateThemeAsync(string id, string theme)
        {
            try
            {
                var collection = await GetUsersCollectionAsync();
                var update = Builders<User>.Update.Set(u => u.Theme, theme);
                var result = await collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating theme for user with ID {UserId}", id);
                throw;
            }
        }
    }
} 