using NewsSite.Server.Models.ArticleAggregate;

namespace NewsSite.Server.Models
{
    public class TopicTrends
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public TimeSpan Period { get; set; }
        public int ArticleCount { get; set; }
        public List<string> RelatedTopics { get; set; } = new();
        public List<EmergingTopic> DailyTrends { get; set; } = new();
        public DateTime AnalyzedAt { get; set; }
    }
} 