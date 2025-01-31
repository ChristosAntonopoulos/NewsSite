//using Microsoft.EntityFrameworkCore;
//using NewsSite.Server.Data;
//using NewsSite.Server.Models;
//using NewsSite.Server.Models.ArticleAggregate;

//namespace NewsSite.Server.Services.Pipelines
//{
//    public interface ITopicTrendAnalyzer
//    {
//        Task<TopicTrends> AnalyzeTrendsAsync(string topic, TimeSpan period);
//        Task<List<EmergingTopic>> IdentifyEmergingTopicsAsync();
//    }

//    public class TopicTrendAnalyzer : ITopicTrendAnalyzer
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<TopicTrendAnalyzer> _logger;

//        public TopicTrendAnalyzer(ApplicationDbContext context, ILogger<TopicTrendAnalyzer> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<TopicTrends> AnalyzeTrendsAsync(string topic, TimeSpan period)
//        {
//            var startDate = DateTime.UtcNow - period;

//            var articles = await _context.ProcessedArticles
//                .Where(a => a.ProcessedAt >= startDate)
//                .Where(a => a.Topics.Contains(topic))
//                .OrderBy(a => a.ProcessedAt)
//                .ToListAsync();

//            return new TopicTrends
//            {
//                Topic = topic,
//                Period = period,
//                ArticleCount = articles.Count,
//                RelatedTopics = IdentifyRelatedTopics(articles),
//                AnalyzedAt = DateTime.UtcNow
//            };
//        }

//        public async Task<List<EmergingTopic>> IdentifyEmergingTopicsAsync()
//        {
//            var recentArticles = await _context.ProcessedArticles
//                .Where(a => a.ProcessedAt >= DateTime.UtcNow.AddDays(-7))
//                .ToListAsync();

//            return AnalyzeEmergingTopics(recentArticles);
//        }



//        private List<string> IdentifyRelatedTopics(List<ProcessedArticle> articles)
//        {
//            // Implement related topics identification logic
//            return new List<string>();
//        }

//        private List<EmergingTopic> AnalyzeEmergingTopics(List<ProcessedArticle> articles)
//        {
//            // Implement emerging topics analysis logic
//            return new List<EmergingTopic>();
//        }
//    }
//}