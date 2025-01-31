//using NewsSite.Server.Interfaces;
//using NewsSite.Server.Models.ArticleAggregate;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace NewsSite.Server.Services.Caching
//{
//    public class InMemoryArticleRepository : IArticleRepository
//    {
//        private readonly ConcurrentDictionary<int, Article> _articles;
//        private int _nextId = 1;

//        public InMemoryArticleRepository()
//        {
//            _articles = new ConcurrentDictionary<int, Article>();
//            InitializeMockData();
//        }

//        private void InitializeMockData()
//        {
//            var mockArticles = new[]
//            {
//                new Article
//                {
//                    Id = "adasd",
//                    Title = "Breaking News: AI Breakthrough",
//                    Summary = "Scientists achieve major breakthrough in AI research",
//                    VerifiedFacts = new List<VerifiedFact> 
//                    { 
//                        new VerifiedFact 
//                        { 
//                            Fact = "New AI model achieves human-level performance",
//                            Confidence = 95,
//                            Sources = new List<ArticleSource> 
//                            { 
//                                new ArticleSource { Title = "Tech Journal", Url = "https://techjournal.com" } 
//                            }
//                        }
//                    },
//                    SourceCount = 2,
//                    Sources = new List<ArticleSource> 
//                    { 
//                        new ArticleSource { Title = "Tech Journal", Url = "https://techjournal.com" },
//                        new ArticleSource { Title = "Science Daily", Url = "https://sciencedaily.com" }
//                    },
//                    FeaturedImage = "ai-breakthrough.jpg",
//                    Version = 1,
//                    WordCount = 500,
//                    Metadata = new Dictionary<string, string> { { "category", "Technology" } },
//                    CreatedAt = DateTime.UtcNow.AddDays(-1),
//                    Content = "Full article content here...",
//                    Url = "https://example.com/ai-breakthrough",
//                    PublishedAt = DateTime.UtcNow.AddDays(-1),
//                    Topics = new List<string> { "AI", "Technology", "Research" }
//                },
//                new Article
//                {
//                    Id = _nextId++,
//                    Title = "Climate Change Report",
//                    Summary = "New study reveals impact of climate change",
//                    VerifiedFacts = new List<VerifiedFact> 
//                    { 
//                        new VerifiedFact 
//                        { 
//                            Fact = "Rising temperatures",
//                            Confidence = 90,
//                            Sources = new List<ArticleSource> 
//                            { 
//                                new ArticleSource { Title = "Environmental Journal", Url = "https://envjournal.com" } 
//                            }
//                        },
//                        new VerifiedFact 
//                        { 
//                            Fact = "Sea level rise",
//                            Confidence = 95,
//                            Sources = new List<ArticleSource> 
//                            { 
//                                new ArticleSource { Title = "Climate Research", Url = "https://climate-research.org" } 
//                            }
//                        }
//                    },
//                    SourceCount = 3,
//                    Sources = new List<ArticleSource> 
//                    { 
//                        new ArticleSource { Title = "Environmental Journal", Url = "https://envjournal.com" },
//                        new ArticleSource { Title = "Climate Research", Url = "https://climate-research.org" },
//                        new ArticleSource { Title = "UN Report", Url = "https://un.org/climate" }
//                    },
//                    FeaturedImage = "climate-change.jpg",
//                    Version = 1,
//                    WordCount = 750,
//                    Metadata = new Dictionary<string, string> { { "category", "Environment" } },
//                    CreatedAt = DateTime.UtcNow.AddDays(-2),
//                }
//            };

//            foreach (var article in mockArticles)
//            {
//                _articles.TryAdd(article.Id, article);
//            }
//        }

//        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
//        {
//            return await Task.FromResult(_articles.Values.ToList());
//        }

//        public async Task<Article?> GetArticleByIdAsync(int id)
//        {
//            _articles.TryGetValue(id, out var article);
//            return await Task.FromResult(article);
//        }

//        public async Task<Article> CreateArticleAsync(Article article)
//        {
//            article.Id = Interlocked.Increment(ref _nextId);
//            article.CreatedAt = DateTime.UtcNow;
//            article.UpdatedAt = DateTime.UtcNow;
//            article.Version = 1;

//            if (_articles.TryAdd(article.Id, article))
//            {
//                return await Task.FromResult(article);
//            }

//            throw new Exception("Failed to create article");
//        }

//        public async Task<bool> UpdateArticleAsync(Article article)
//        {
//            article.UpdatedAt = DateTime.UtcNow;
//            article.Version++;

//            return await Task.FromResult(_articles.TryUpdate(article.Id, article, _articles[article.Id]));
//        }

//        public async Task<bool> DeleteArticleAsync(int id)
//        {
//            return await Task.FromResult(_articles.TryRemove(id, out _));
//        }

//        Task<Article> IArticleRepository.UpdateArticleAsync(Article article)
//        {
//            throw new NotImplementedException();
//        }

//        Task IArticleRepository.DeleteArticleAsync(int id)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}