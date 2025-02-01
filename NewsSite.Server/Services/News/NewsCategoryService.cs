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
                        var title = result.GetProperty("title").GetString();
                        var snippet = result.TryGetProperty("snippet", out var snippetElement) 
                            ? snippetElement.GetString() 
                            : "";
                        var link = result.GetProperty("link").GetString();
                        var source = result.GetProperty("source");
                        var sourceName = source.GetProperty("name").GetString();
                        var sourceIcon = source.GetProperty("icon").GetString();
                        var authors = source.GetProperty("authors").EnumerateArray()
                            .Select(a => a.GetString())
                            .Where(a => !string.IsNullOrEmpty(a))
                            .ToList();
                        var thumbnail = result.TryGetProperty("thumbnail", out var thumbElement) 
                            ? thumbElement.GetString() 
                            : "";
                        var publishedDate = DateTime.UtcNow;

                        // Extract keywords using OpenAI
                        var keywords = await ExtractKeywords(title, snippet);

                        // Search for related news articles using keywords
                        var additionalSources = await SearchRelatedNews(title);

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

                        // Create initial source
                        var initialSource = new ArticleSource
                        {
                            Title = new LocalizedContent { En = sourceName ?? "" },
                            Url = link ?? "",
                            Name = sourceName ?? "",
                            Type = "primary"
                        };

                        // Combine all sources
                        var allSources = new List<ArticleSource> { initialSource };
                        if (additionalSources.Count > 3)
                        {
                            allSources.AddRange(additionalSources);
                        }

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
                            Sources = allSources,
                            SourceCount = allSources.Count,
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

        //TP DP : 60 %
        private async Task<bool> HasSimilarArticle(List<string> keywords)
        {
            const double SIMILARITY_THRESHOLD = 0.6; // 60% similarity threshold
            var thresholdDate = DateTime.UtcNow.AddDays(-3);
            var articles = await _articleRepository.GetArticlesAsync(1, 10);
            
            return articles.Items.Any(article => 
            {
                if (article.PublishedAt < thresholdDate || !article.Metadata.ContainsKey("keywords"))
                    return false;

                var articleKeywords = article.Metadata["keywords"]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLowerInvariant())
                    .ToList();

                var normalizedNewKeywords = keywords
                    .Select(k => k.Trim().ToLowerInvariant())
                    .ToList();

                // Count matching keywords
                var matchingKeywords = normalizedNewKeywords
                    .Intersect(articleKeywords)
                    .Count();

                // Calculate similarity ratio
                var similarityRatio = (double)matchingKeywords / Math.Max(normalizedNewKeywords.Count, articleKeywords.Count);

                _logger.LogInformation(
                    $"Keyword similarity check - Article: {article.Title.En}, " +
                    $"Similarity: {similarityRatio:P}, " +
                    $"Matching Keywords: {matchingKeywords}/{Math.Max(normalizedNewKeywords.Count, articleKeywords.Count)}"
                );

                return similarityRatio >= SIMILARITY_THRESHOLD;
            });
        }

        private async Task<List<VerifiedFact>> ExtractAndVerifyFacts(string title, string snippet)
        {
            var prompt = @"You are a combined fact-checker and credibility analyst.Analyze the following news article and extract key factual claims. 
             Extract verifiable facts:
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
            var prompt = @"Extract key wards from the title and the snippet.
Format your response as a JSON object with a 'keywords' property containing an array of at least 6 strings.
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
    ""summary"": ""[100 -150  word comprehensive summary in a single line]""
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

        private async Task<List<ArticleSource>> SearchRelatedNews(string title)
        {
            try
            {
                var searchParams = new SearchParameters
                {
                    Num = 30,
                    Engine = "google_news",
                    AdditionalParameters = new Dictionary<string, string>
                    {
                        { "tbm", "nws" }
                    }
                };

                // Create a search query using the keywords
                var searchQuery = "related:" + title;
                var searchResults = await _serpApiService.SearchAsync(searchQuery, searchParams);

                var sources = new List<ArticleSource>();
                const double TITLE_SIMILARITY_THRESHOLD = 0.4; // 40% similarity threshold

                if (searchResults.TryGetValue("news_results", out var resultsObj) && 
                    resultsObj is JsonElement resultsElement && 
                    resultsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var result in resultsElement.EnumerateArray())
                    {
                        try
                        {
                            var sourceUrl = result.GetProperty("link").GetString();
                            var sourceName = result.GetProperty("source").GetProperty("name").GetString();
                            var sourceTitle = result.GetProperty("title").GetString();

                            // Calculate title similarity
                            var titleSimilarity = CalculateTitleSimilarity(title, sourceTitle);
                            
                            _logger.LogInformation(
                                $"Title similarity check - Original: {title}, Source: {sourceTitle}, " +
                                $"Similarity: {titleSimilarity:P}"
                            );

                            // Only add sources with sufficient title similarity
                            if (titleSimilarity >= TITLE_SIMILARITY_THRESHOLD)
                            {
                                sources.Add(new ArticleSource
                                {
                                    Url = sourceUrl,
                                    Name = sourceName,
                                    Type = "news",
                                    Title = new LocalizedContent { En = sourceTitle }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error processing related news result");
                            continue;
                        }
                    }
                }

                // Only return sources if we have more than 3
                return sources.Count > 3 ? sources : new List<ArticleSource>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for related news");
                return new List<ArticleSource>();
            }
        }

        private double CalculateTitleSimilarity(string originalTitle, string sourceTitle)
        {
            if (string.IsNullOrWhiteSpace(originalTitle) || string.IsNullOrWhiteSpace(sourceTitle))
                return 0;

            // Normalize titles: convert to lowercase and split into words
            var originalWords = originalTitle.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '-', '(', ')' }, 
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(w => w.Length > 3) // Only consider words longer than 3 characters
                .ToList();

            var sourceWords = sourceTitle.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '-', '(', ')' }, 
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(w => w.Length > 3) // Only consider words longer than 3 characters
                .ToList();

            // Count matching words
            var matchingWords = originalWords.Intersect(sourceWords).Count();

            // Calculate similarity ratio based on the longer title
            return (double)matchingWords / Math.Max(originalWords.Count, sourceWords.Count);
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