using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NewsSite.Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("language")]
        public string Language { get; set; } = "en";

        [BsonElement("theme")]
        public string Theme { get; set; } = "light";

        [BsonElement("googleId")]
        public string? GoogleId { get; set; }

        [BsonElement("redditId")]
        public string? RedditId { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLoginAt")]
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    }
} 