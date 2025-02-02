using Microsoft.Extensions.Logging;
using NewsSite.Server.Services.SerpAi;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using NewsSite.Server.Models.Common;
using NewsSite.Server.Models.Pipeline;
using System.Text.Json;
using NewsSite.Server.Interfaces;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NewsSite.Server.Services.News
{
    public class NewsCategoryService : INewsCategoryService
    {
        private readonly ISerpApiService _serpApiService;
        private readonly IOpenAIService _openAiService;
        private readonly ILogger<NewsCategoryService> _logger;
        private readonly IArticleRepository _articleRepository;

        public NewsCategoryService(
            ISerpApiService serpApiService,
            IOpenAIService openAiService,
            IArticleRepository articleRepository,
            ILogger<NewsCategoryService> logger)
        {
            _serpApiService = serpApiService;
            _openAiService = openAiService;
            _articleRepository = articleRepository;
            _logger = logger;
        }

        public async Task<NewsSearchResult> GetNewsByCategory(string category)
        {
            try
            {
                _logger.LogInformation($"Fetching news for category: {category}");

                var searchParams = new SearchParameters
                {
                    Num = 20,
                    Engine = "google_news",
                    AdditionalParameters = new Dictionary<string, string>
                    {
                        { "tbm", "nws" }
                    }
                };

                var initialResults = await _serpApiService.SearchAsync($"{category} news", searchParams);
                var articles = await ProcessSearchResults(initialResults, category);

                return new NewsSearchResult
                {
                    InitialResults = articles.Select(a => new Dictionary<string, object>
                    {
                        { "title", a.Title.GetContent("en") },
                        { "summary", a.Summary.GetContent("en") },
                        { "url", a.Url["en"] },
                        { "publishedAt", a.PublishedAt },
                        { "category", a.Category.Name },
                        { "analysis", a.Analysis },
                        { "keywords", a.Metadata["keywords"] },
                        { "verifiedFacts", a.VerifiedFacts }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching news for category: {category}");
                throw;
            }
        }

        private async Task<List<Article>> ProcessSearchResults(Dictionary<string, object> searchResults, string category)
        {
            var processedArticles = new List<Article>();

            if (searchResults.TryGetValue("news_results", out var resultsObj) && 
                resultsObj is JsonElement resultsElement && 
                resultsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var result in resultsElement.EnumerateArray().Take(30))
                {
                    try 
                    {
                        // Use TryGetProperty for all property accesses
                        if (!result.TryGetProperty("title", out var titleElement) || 
                            !result.TryGetProperty("link", out var linkElement) ||
                            !result.TryGetProperty("source", out var sourceElement))
                        {
                            _logger.LogWarning("Required properties missing from search result");
                            continue;
                        }

                        var title = titleElement.GetString() ?? "";
                        var snippet = result.TryGetProperty("snippet", out var snippetElement) 
                            ? snippetElement.GetString() ?? ""
                            : "";
                        var link = linkElement.GetString() ?? "";

                        // Handle source properties safely
                        var sourceName = sourceElement.TryGetProperty("name", out var nameElement) 
                            ? nameElement.GetString() ?? "" 
                            : "";
                        var sourceIcon = sourceElement.TryGetProperty("icon", out var iconElement) 
                            ? iconElement.GetString() ?? "" 
                            : "";

                        // Handle authors array safely
                        var authors = new List<string>();
                        if (sourceElement.TryGetProperty("authors", out var authorsElement) && 
                            authorsElement.ValueKind == JsonValueKind.Array)
                        {
                            authors = authorsElement.EnumerateArray()
                                .Select(a => a.GetString())
                                .Where(a => !string.IsNullOrEmpty(a))
                                .ToList();
                        }

                        var thumbnail = result.TryGetProperty("thumbnail", out var thumbElement) 
                            ? thumbElement.GetString() ?? ""
                            : "";
                        var publishedDate = DateTime.UtcNow;
                        var relatedSources = await GetRelatedSources(title);

                        // Skip if there aren't enough related sources
                        if (relatedSources.Count < 3  || relatedSources.Count > 15)
                        {
                            _logger.LogInformation($"Skipping article  related sources: {title} number of sources: {relatedSources.Count} ");
                            continue;
                        }

                        // Extract keywords using OpenAI
                        var keywords = await ExtractKeywords(title, snippet);

                        // Check for similar articles in the last 3 days
                        if (await HasSimilarArticle(keywords.Keywords.ToList()))
                        {
                            _logger.LogInformation($"Similar article found for: {title}");
                            continue;
                        }

                        // Extract and verify facts
                        var verifiedFacts = await ExtractAndVerifyFacts(title, snippet);
                        
                        // Skip articles with insufficient verified facts
                        if (verifiedFacts.Count < 3)
                        {
                            _logger.LogInformation($"Skipping article with insufficient facts: {title}");
                            continue;
                        }

                        // Generate enhanced content using verified facts
                        var enhancedContent = await GenerateEnhancedContent(title, snippet, verifiedFacts);
                        if (enhancedContent == null)
                        {
                            _logger.LogWarning($"Failed to generate enhanced content for: {title}");
                            continue;
                        }

                        // Create translations for the enhanced content
                       // var (translatedTitle, translatedSummary) = await TranslateContent(
                       //     enhancedContent.Title, 
                       //     enhancedContent.Summary
                       // );

                        // Generate images using DALL-E
                        var imageUrls = await GenerateNewsImages(enhancedContent.Title, enhancedContent.Summary);

                        // Combine DALL-E images with thumbnail if exists
                        var allImages = new List<string>();
                        allImages.AddRange(imageUrls);
                        if (!string.IsNullOrEmpty(thumbnail))
                        {
                            allImages.Add(thumbnail);
                        }

                        // Get related sources based on title
                        
                        var article = new Article
                        {
                            Title = new LocalizedContent { En = enhancedContent.Title ?? "" },
                            Summary = new LocalizedContent { En = enhancedContent.Summary ?? "" },
                            Url = new Dictionary<string, string> { { "en", link ?? "" } },
                            PublishedAt = publishedDate,
                            Category = new Category { 
                                Name = new LocalizedContent { En = category, El = category }
                            },
                            Analysis = keywords.ToString(),
                            ProcessedAt = DateTime.UtcNow,
                            VerifiedFacts = verifiedFacts,
                            FeaturedImages = allImages,
                            Content = new LocalizedContent { En = enhancedContent.Summary ?? "", El = "" },
                            Sources = relatedSources,
                            SourceCount = relatedSources.Count,
                            Metadata = new Dictionary<string, string>  
                            {
                                { "keywords", string.Join(",", keywords.ToString()) },
                                { "sourceName", sourceName ?? "" },
                                { "sourceIcon", sourceIcon ?? "" },
                                { "authors", string.Join(",", authors) },
                                { "thumbnail", thumbnail ?? "" }
                            }
                        };

                        await _articleRepository.CreateArticleAsync(article);
                        processedArticles.Add(article);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing search result");
                        continue;
                    }
                }
            }

            return processedArticles;
        }

        private async Task<bool> HasSimilarArticle(List<string> keywords)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-3);
            var articles = await _articleRepository.GetArticlesAsync(1, 10);
            
            return articles.Items.Any(a => 
                a.PublishedAt >= thresholdDate && 
                a.Metadata.ContainsKey("keywords") &&
                keywords.Any(k => a.Metadata["keywords"].Contains(k)));
        }

        private async Task<List<VerifiedFact>> ExtractAndVerifyFacts(string title, string snippet)
        {
            var prompt = @"You are a combined fact-checker and credibility analyst.Analyze the following news article and extract key factual claims. 
             Extract the 3 more important verifiable facts:
  - Focus on concrete events, numbers, dates, and direct quotes.
  - Exclude opinions, predictions, or speculative content.
 1) Extract only verifiable facts—focus on concrete events, numbers, dates, direct quotes.  
        Exclude any opinions, predictions, or speculative content.  
2) Assess credibility based on:
        Historical accuracy
        Editorial standards
        Reputation
        Fact-checking practices

Format your response as a JSON array of objects with 'fact' and 'confidence' properties.
The confidence should be a number from 1-10.
Example format:
[
    {
        ""fact"": ""Apple released new iPhone"",
        ""confidence"": 9
    }
]

Article Title: " + title + "\nSummary: " + snippet;

            var facts = await GetOpenAiJsonResponse<List<FactResponse>>(prompt, 300);
            
            return facts?.Select(f => new VerifiedFact
            {
                Fact = new LocalizedContent { En = f.Fact },
                Confidence = f.Confidence,
                Sources = new List<ArticleSource>()
            }).ToList() ?? new List<VerifiedFact>();
        }

        private async Task<KeywordsResponse> ExtractKeywords(string title, string snippet)
        {
            var prompt = @"Extract key topics or themes from this news article.
Format your response as a JSON object with a 'keywords' property containing an array of 3-5 strings.
Example format:
{
    ""keywords"": [""AI"", ""Technology"", ""Innovation""]
}

Article Title: " + title + "\nSummary: " + snippet;

            return await GetOpenAiJsonResponse<KeywordsResponse>(prompt, 150);
        }

        private async Task<(string title, string summary)> TranslateContent(string title, string snippet)
        {
            var prompt = @"Translate the following text to Greek.
Format your response as a JSON object with 'title' and 'summary' properties.
Example format:
{
    ""title"": ""[Greek translation of title]"",
    ""summary"": ""[Greek translation of summary]""
}

Original Title: " + title + "\nOriginal Summary: " + snippet;

            var translation = await GetOpenAiJsonResponse<TranslationResponse>(prompt, 500);
            return (translation?.Title ?? "", translation?.Summary ?? "");
        }

        private async Task<EnhancedContent> GenerateEnhancedContent(
            string originalTitle, 
            string originalSummary, 
            List<VerifiedFact> facts)
        {
            var factsJson = JsonSerializer.Serialize(facts.Select(f => f.Fact.En));
            var prompt = @$"Based on these verified facts and the original content, create an enhanced news article.
The response should be a JSON object with 'title' and 'summary' properties.
The title should be unique and engaging(under 12 words)  Avoid sensationalism or copying source headlines Use neutral language and focus on the most important fact..
Write a summary in an inverted pyramid structure (most important information first).
Use neutral and objective language and Reflect confirmed information only, avoiding sensationalism.

Important: The summary should be a single line without line breaks.

Original Title: {originalTitle}
Original Summary: {originalSummary}
Verified Facts: {factsJson}

Example format:
{{
    ""title"": ""[Unique and engaging title]"",
    ""summary"": ""[100-150 word comprehensive summary in a single line]""
}}";

            return await GetOpenAiJsonResponse<EnhancedContent>(prompt, 1000, 0.7f);
        }

        private async Task<T> GetOpenAiJsonResponse<T>(string prompt, int maxTokens, float temperature = 0.3f, string model = "gpt-3.5-turbo-instruct") where T : class
        {
            var completionRequest = new CompletionCreateRequest
            {
                Model = model,
                Prompt = prompt,
                MaxTokens = maxTokens,
                Temperature = temperature
            };

            var completionResult = await _openAiService.Completions.CreateCompletion(completionRequest);
            if (!completionResult.Successful)
            {
                _logger.LogError($"OpenAI API error: {completionResult.Error?.Message}");
                return null;
            }

            try
            {
                var jsonResponse = completionResult.Choices[0].Text.Trim();
                _logger.LogDebug($"Raw OpenAI response: {jsonResponse}");

                // Handle both array and object responses
                var isArray = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>);
                
                if (isArray)
                {
                    var startIndex = jsonResponse.IndexOf('[');
                    var endIndex = jsonResponse.LastIndexOf(']');
                    
                    if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
                    {
                        _logger.LogError("Invalid JSON array response format from OpenAI");
                        return null;
                    }
                    
                    jsonResponse = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                }
                else
                {
                    var startIndex = jsonResponse.IndexOf('{');
                    var endIndex = jsonResponse.LastIndexOf('}');
                    
                    if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
                    {
                        _logger.LogError("Invalid JSON object response format from OpenAI");
                        return null;
                    }
                    
                    jsonResponse = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                }

                _logger.LogDebug($"Cleaned JSON response: {jsonResponse}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                return JsonSerializer.Deserialize<T>(jsonResponse, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI response: {Response}", completionResult.Choices[0].Text);
                return null;
            }
        }

        private class FactResponse
        {
            public string Fact { get; set; }
            public int Confidence { get; set; }
        }

        private class TranslationResponse
        {
            public string Title { get; set; }
            public string Summary { get; set; }
        }

        private class KeywordsResponse
        {
            public string[] Keywords { get; set; }
        }

        private class EnhancedContent
        {
            public string Title { get; set; }
            public string Summary { get; set; }
        }

        private async Task<List<string>> GenerateNewsImages(string title, string snippet)
        {
            try
            {
                var prompt = $"Create a photorealistic news article image for the following headline: {title}. Context: {snippet}";
                
                var request = new ImageCreateRequest
                {
                    Prompt = prompt,
                    Model = "dall-e-3",
                    Quality = "standard",
                    Size = "1024x1024",
                    Style = "vivid",
                    N = 1,
                    ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url
                };

                var imageResult = await _openAiService.Image.CreateImage(request);
                
                if (!imageResult.Successful)
                {
                    _logger.LogError($"DALL-E image generation failed: {imageResult.Error?.Message}");
                    return new List<string>();
                }

                var localUrls = new List<string>();
                foreach (var result in imageResult.Results)
                {
                    try
                    {
                        // Create a unique filename using a timestamp and a GUID
                        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                        var fileName = $"{timestamp}_{uniqueId}.png";
                        
                        // Ensure the images directory exists
                        var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "news");
                        Directory.CreateDirectory(imagesPath);
                        
                        // Download and save the image
                        using (var httpClient = new HttpClient())
                        {
                            var imageBytes = await httpClient.GetByteArrayAsync(result.Url);
                            var localPath = Path.Combine(imagesPath, fileName);
                            await File.WriteAllBytesAsync(localPath, imageBytes);
                            
                            // Generate the local URL
                            var localUrl = $"/images/news/{fileName}";
                            localUrls.Add(localUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving DALL-E image locally");
                        // Continue with next image if one fails
                        continue;
                    }
                }

                return localUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating news images with DALL-E");
                return new List<string>();
            }
        }

        private async Task<List<ArticleSource>> GetRelatedSources(string title)
        {
            try
            {
                var searchParams = new SearchParameters
                {
                    Num = 20, // Limit to 5 related articles
                    Engine = "google_news",
                    AdditionalParameters = new Dictionary<string, string>
                    {
                        { "tbm", "nws" }
                    }
                };

                var relatedResults = await _serpApiService.SearchAsync($"{title}", searchParams);
                var sources = new List<ArticleSource>();

                if (relatedResults.TryGetValue("news_results", out var resultsObj) && 
                    resultsObj is JsonElement resultsElement && 
                    resultsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var result in resultsElement.EnumerateArray())
                    {
                        try 
                        {
                            var relatedTitle = result.GetProperty("title").GetString();
                            var relatedLink = result.GetProperty("link").GetString();
                            
                            // Get source name from the nested source object
                            var sourceName = "";
                            if (result.TryGetProperty("source", out var sourceObj) && 
                                sourceObj.TryGetProperty("name", out var nameElement))
                            {
                                sourceName = nameElement.GetString() ?? "";
                            }

                            // Calculate similarity between titles
                            if (CalculateTitleSimilarity(title, relatedTitle) >= 0.6) // 60% similarity threshold
                            {
                                sources.Add(new ArticleSource
                                {
                                    Title = new LocalizedContent { En = relatedTitle },
                                    Url = relatedLink,
                                    Name = sourceName
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing related source");
                            continue;
                        }
                    }
                }

                return sources;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching related sources for title: {Title}", title);
                return new List<ArticleSource>();
            }
        }

        private double CalculateTitleSimilarity(string title1, string title2)
        {
            // Normalize titles
            title1 = title1.ToLower().Trim();
            title2 = title2.ToLower().Trim();

            if (string.IsNullOrEmpty(title1) || string.IsNullOrEmpty(title2))
                return 0;

            // Calculate Levenshtein distance
            var distance = ComputeLevenshteinDistance(title1, title2);
            var maxLength = Math.Max(title1.Length, title2.Length);
            
            // Convert distance to similarity score (0 to 1)
            return 1 - ((double)distance / maxLength);
        }

        private int ComputeLevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            // Initialize first row and column
            for (var i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            for (var j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            // Fill rest of the matrix
            for (var i = 1; i <= s1.Length; i++)
            {
                for (var j = 1; j <= s2.Length; j++)
                {
                    var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }
    }

    public interface INewsCategoryService
    {
        Task<NewsSearchResult> GetNewsByCategory(string category);
    }

    public class NewsSearchResult
    {
        public List<Dictionary<string, object>> InitialResults { get; set; }
    }
} 