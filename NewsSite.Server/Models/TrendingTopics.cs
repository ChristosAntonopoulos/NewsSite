using NewsSite.Server.Models.ArticleAggregate;

namespace NewsSite.Server.Models
{
    public class TrendingTopics
    {
        public List<SocialMediaTopic> TwitterTopics { get; set; } = new();
        public List<SocialMediaTopic> RedditTopics { get; set; } = new();
    }
} 