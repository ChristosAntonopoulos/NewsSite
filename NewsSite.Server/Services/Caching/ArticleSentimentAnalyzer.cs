//using NewsSite.Server.Models.ArticleAggregate;
//using System.Diagnostics;

//namespace NewsSite.Server.Services.Caching
//{
//    public interface IArticleSentimentAnalyzer
//    {
//        Task<ArticleSentiment> AnalyzeArticleAsync(Article article);
//        Task<List<ArticleSentiment>> AnalyzeBatchAsync(List<Article> articles);
//    }

//    public class ArticleSentimentAnalyzer : IArticleSentimentAnalyzer
//    {
//        private readonly IOpenAIService _openAIService;
//        private readonly ILogger<ArticleSentimentAnalyzer> _logger;
//        private readonly IMetricsService _metricsService;

//        public ArticleSentimentAnalyzer(
//            IOpenAIService openAIService,
//            ILogger<ArticleSentimentAnalyzer> logger,
//            IMetricsService metricsService)
//        {
//            _openAIService = openAIService;
//            _logger = logger;
//            _metricsService = metricsService;
//        }

//        public async Task<ArticleSentiment> AnalyzeArticleAsync(Article article)
//        {
//            var stopwatch = Stopwatch.StartNew();
//            try
//            {
//                var prompt = $"Analyze the sentiment and key themes of this article:\n\nTitle: {article.Title}\n\nContent: {article.Content}";
//                var analysis = await _openAIService.GenerateAnalysisAsync(prompt);

//                return new ArticleSentiment
//                {
//                    ArticleId = article.Id,
//                    Article = article,
//                    Sentiment = ParseSentiment(analysis),
//                    KeyThemes = ExtractKeyThemes(analysis),
//                    RawAnalysis = analysis,
//                };
//            }
//            finally
//            {
//                _metricsService.TrackApiCallDuration("SentimentAnalysis", stopwatch.Elapsed);
//            }
//        }

//        public async Task<List<ArticleSentiment>> AnalyzeBatchAsync(List<Article> articles)
//        {
//            var tasks = articles.Select(AnalyzeArticleAsync).ToList();
//            var results = await Task.WhenAll(tasks);
//            return results.ToList();
//        }

//        private string ParseSentiment(string analysis)
//        {
//            // Implement sentiment parsing logic
//            return "neutral";
//        }

//        private List<string> ExtractKeyThemes(string analysis)
//        {
//            // Implement theme extraction logic
//            return new List<string>();
//        }
//    }
//}