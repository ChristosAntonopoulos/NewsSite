using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using NewsSite.Server.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsSite.Server.Interfaces
{
    public interface IArticleRepository
    {
        Task<PagedResult<Article>> GetArticlesAsync(int page, int pageSize, string categoryId = null, string topicId = null, string language = "en");
        Task<Article> GetArticleByIdAsync(string id);
        Task<Article> CreateArticleAsync(Article article);
        Task<bool> UpdateArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(string id);
        Task<IEnumerable<Category>> GetCategoriesAsync(string language = "en");
        Task<Category> GetCategoryByIdAsync(string id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(string id);
        Task<IEnumerable<SocialMediaTopic>> GetTopicsByCategoryAsync(string categoryId, string language = "en");
        Task<PagedResult<Article>> GetArticlesByIdsAsync(List<string> ids, int page, int pageSize);
    }
} 