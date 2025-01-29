public class NewsServiceSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://serpapi.com/search";
    public int MinRequestInterval { get; set; } = 5;
    public int MaxRetries { get; set; } = 3;
    public int ResultsPerPage { get; set; } = 30;
    
    public Dictionary<string, string[]> CategoryKeywords { get; set; } = new()
    {
        ["Technology"] = new[] { "tech", "technology", "software", "hardware", "ai", "digital" },
        ["Business"] = new[] { "business", "economy", "market", "finance", "stock" },
        ["Science"] = new[] { "science", "research", "study", "discovery" },
        ["Health"] = new[] { "health", "medical", "healthcare", "medicine" }
    };
} 