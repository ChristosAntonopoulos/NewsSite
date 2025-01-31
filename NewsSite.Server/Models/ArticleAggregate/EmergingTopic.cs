namespace NewsSite.Server.Models.ArticleAggregate
{
    public class EmergingTopic
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public double GrowthRate { get; set; }
        public int ArticleCount { get; set; }
        public List<string> RelatedTopics { get; set; } = new();
        public DateTime IdentifiedAt { get; set; }
    }
}