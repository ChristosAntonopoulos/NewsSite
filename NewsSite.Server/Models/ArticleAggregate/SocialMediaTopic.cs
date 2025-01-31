using NewsSite.Server.Models.Common;

namespace NewsSite.Server.Models.ArticleAggregate
{
    public class SocialMediaTopic
    {
        public string TopicId { get; set; }
        public string CategoryId { get; set; }
        public string Title { get; set; }
        public string Platform { get; set; }
        public int TrendScore { get; set; }
        public string Url { get; set; }
    }
} 