using NewsSite.Server.Models;

public interface INewsService
{
    Task<IEnumerable<ProcessedArticle>> FetchTechNewsAsync();
    Task<IEnumerable<ProcessedArticle>> FetchNewsByCategory(string category);
    Task<IEnumerable<ProcessedArticle>> SearchNewsAsync(string query);
    Task<ProcessedArticle?> GetArticleByUrlAsync(string url);
    Task<IEnumerable<ProcessedArticle>> GetRelatedArticlesAsync(ProcessedArticle article, int count = 5);
} 