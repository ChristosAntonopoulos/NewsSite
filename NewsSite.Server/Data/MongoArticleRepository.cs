using MongoDB.Driver;
using NewsSite.Server.Interfaces;
using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using NewsSite.Server.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

public class MongoArticleRepository : IArticleRepository
{
    private readonly IMongoDbConnectionService _connectionService;
    private readonly ILogger<MongoArticleRepository> _logger;
    private IMongoCollection<Article>? _articles;
    private IMongoCollection<Category>? _categories;
    private IMongoCollection<Topic>? _topics;

    public MongoArticleRepository(
        IMongoDbConnectionService connectionService,
        ILogger<MongoArticleRepository> logger)
    {
        _connectionService = connectionService;
        _logger = logger;
    }

    private async Task<IMongoCollection<Article>> GetArticlesCollectionAsync()
    {
        try
        {
            if (_articles == null)
            {
                var database = await _connectionService.GetDatabaseAsync();
                _articles = database.GetCollection<Article>("Articles");
            }
            return _articles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Articles collection");
            throw;
        }
    }

    private async Task<IMongoCollection<Category>> GetCategoriesCollectionAsync()
    {
        try
        {
            if (_categories == null)
            {
                var database = await _connectionService.GetDatabaseAsync();
                _categories = database.GetCollection<Category>("Categories");
            }
            return _categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Categories collection");
            throw;
        }
    }

    private async Task<IMongoCollection<Topic>> GetTopicsCollectionAsync()
    {
        try
        {
            if (_topics == null)
            {
                var database = await _connectionService.GetDatabaseAsync();
                _topics = database.GetCollection<Topic>("Topics");
            }
            return _topics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Topics collection");
            throw;
        }
    }

    public async Task<Article?> GetArticleByIdAsync(string id)
    {
        try
        {
            var collection = await GetArticlesCollectionAsync();
            return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article with ID {ArticleId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        try
        {
            var collection = await GetArticlesCollectionAsync();
            return await collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles from MongoDB");
            return Enumerable.Empty<Article>();
        }
    }

    public async Task<Article> CreateArticleAsync(Article article)
    {
        try
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            // Get categories collection
            var categoriesCollection = await GetCategoriesCollectionAsync();

            // Check if category exists
            var filter = Builders<Category>.Filter.Or(
                Builders<Category>.Filter.And(
                    Builders<Category>.Filter.Exists($"Name.en", true),
                    Builders<Category>.Filter.Eq($"Name.en", article.Category.Name.En)
                ),
                Builders<Category>.Filter.And(
                    Builders<Category>.Filter.Exists($"Name.el", true),
                    Builders<Category>.Filter.Eq($"Name.el", article.Category.Name.El)
                )
            );

            var existingCategory = await categoriesCollection.Find(filter).FirstOrDefaultAsync();

            // If category doesn't exist, create it
            if (existingCategory == null)
            {
                await categoriesCollection.InsertOneAsync(article.Category);
                _logger.LogInformation($"Created new category: {article.Category.Name.En}");
            }
            else
            {
                // Use existing category ID
                article.Category.Id = existingCategory.Id;
                article.CategoryId = existingCategory.Id;
            }

            var collection = await GetArticlesCollectionAsync();
            await collection.InsertOneAsync(article);
            return article;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article in MongoDB");
            throw;
        }
    }

    public async Task<bool> UpdateArticleAsync(Article article)
    {
        try
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            var collection = await GetArticlesCollectionAsync();
            var result = await collection.ReplaceOneAsync(x => x.Id == article.Id, article);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article with ID {ArticleId} in MongoDB", article?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteArticleAsync(string id)
    {
        try
        {
            var collection = await GetArticlesCollectionAsync();
            var result = await collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article with ID {ArticleId} from MongoDB", id);
            throw;
        }
    }

    public async Task<PagedResult<Article>> GetArticlesAsync(int page, int pageSize, string categoryId = null, string topicId = null, string languageCode = "en")
    {
        try
        {
            var collection = await GetArticlesCollectionAsync();
            var filter = Builders<Article>.Filter.Empty;

            if (!string.IsNullOrEmpty(categoryId))
                filter &= Builders<Article>.Filter.Eq(x => x.CategoryId, categoryId);
            if (!string.IsNullOrEmpty(topicId))
                filter &= Builders<Article>.Filter.Eq(x => x.TopicId, topicId);

            // Add language-specific filter
            filter &= Builders<Article>.Filter.Or(
                Builders<Article>.Filter.Exists($"Title.{languageCode}", true),
                Builders<Article>.Filter.Exists("Title.en", true)  // Fallback to English
            );

            var totalItems = await collection.CountDocumentsAsync(filter);
            var articles = await collection.Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<Article>
            {
                Items = articles,
                Page = page,
                PageSize = pageSize,
                TotalItems = (int)totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged articles");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync(string languageCode = "en")
    {
        try
        {
            var collection = await GetCategoriesCollectionAsync();
            var filter = Builders<Category>.Filter.Or(
                Builders<Category>.Filter.Exists($"Name.{languageCode}", true),
                Builders<Category>.Filter.Exists("Name.en", true)  // Fallback to English
            );
            return await collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            throw;
        }
    }

    public async Task<IEnumerable<SocialMediaTopic>> GetTopicsByCategoryAsync(string categoryId, string languageCode = "en")
    {
        try
        {
            var collection = await GetTopicsCollectionAsync();
            var filter = Builders<Topic>.Filter.And(
                Builders<Topic>.Filter.Eq(x => x.CategoryId, categoryId),
                Builders<Topic>.Filter.Or(
                    Builders<Topic>.Filter.Exists($"Name.{languageCode}", true),
                    Builders<Topic>.Filter.Exists("Name.en", true)
                )
            );
            var topics = await collection.Find(filter).ToListAsync();
            
            return topics.Select(t => new SocialMediaTopic
            {
                TopicId = t.Id,
                CategoryId = t.CategoryId,
                Title = t.Name.En,
                Platform = "General",
                TrendScore = 0
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving topics for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        try
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var collection = await GetCategoriesCollectionAsync();
            await collection.InsertOneAsync(category);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category in MongoDB");
            throw;
        }
    }

    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        try
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var collection = await GetCategoriesCollectionAsync();
            var result = await collection.ReplaceOneAsync(x => x.Id == category.Id, category);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category with ID {CategoryId}", category?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        try
        {
            var collection = await GetCategoriesCollectionAsync();
            var result = await collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category with ID {CategoryId}", id);
            throw;
        }
    }

    public async Task<Topic> CreateTopicAsync(Topic topic)
    {
        try
        {
            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var collection = await GetTopicsCollectionAsync();
            await collection.InsertOneAsync(topic);
            return topic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating topic in MongoDB");
            throw;
        }
    }

    public async Task<bool> UpdateTopicAsync(Topic topic)
    {
        try
        {
            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var collection = await GetTopicsCollectionAsync();
            var result = await collection.ReplaceOneAsync(x => x.Id == topic.Id, topic);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating topic with ID {TopicId}", topic?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteTopicAsync(string id)
    {
        try
        {
            var collection = await GetTopicsCollectionAsync();
            var result = await collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting topic with ID {TopicId}", id);
            throw;
        }
    }

    public async Task<Category> GetCategoryByIdAsync(string id)
    {
        try
        {
            var collection = await GetCategoriesCollectionAsync();
            return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
            throw;
        }
    }

    public async Task<PagedResult<Article>> GetArticlesByIdsAsync(List<string> ids, int page, int pageSize)
    {
        try
        {
            var collection = await GetArticlesCollectionAsync();
            var filter = Builders<Article>.Filter.In(x => x.Id, ids);
            var totalItems = await collection.CountDocumentsAsync(filter);
            
            var articles = await collection.Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<Article>
            {
                Items = articles,
                TotalItems = (int)totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles by IDs");
            throw;
        }
    }
}