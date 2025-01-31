using System.Collections.Generic;

namespace NewsSite.Server.Models.PipelineAggregate
{
    public class SerpApiStepConfig
    {
        public string ApiKey { get; set; }
        public string SearchQuery { get; set; } = "technology news";
        public string Engine { get; set; } = "google_news";
        public int MaxResults { get; set; } = 10;
        public bool IncludeImages { get; set; } = true;
        public Dictionary<string, string> AdditionalParameters { get; set; } = new();
    }

    public class GptStepConfig
    {
        public string ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4";
        public string SystemPrompt { get; set; }
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
    }

    public static class PipelinePrompts
    {
        public static string FactCheckPrompt = @"You are a combined fact-checker and credibility analyst. Given the article text below, 
perform the following steps:
1) Extract only verifiable facts—focus on concrete events, numbers, dates, direct quotes.
   Exclude any opinions, predictions, or speculative content.
2) Assess the article's source credibility on a scale from 0.0 to 1.0, based on:
   - Historical accuracy
   - Editorial standards
   - Reputation
   - Fact-checking practices

Output Format:
Facts:
 - Provide each fact on a new line.
Credibility:
 - Provide a single numeric score between 0.0 and 1.0.
Do not include any additional commentary.";

        public static string TitlePrompt = @"You are a professional news editor. Create a unique headline under 12 words, 
based on the verified facts {{facts}} extracted from the article. 
Never copy source headlines, avoid sensationalism, and use neutral language. 
Focus on the primary verified fact.";

        public static string SummaryPrompt = @"You are a professional technology journalist. Write a neutral summary (up to 100 words) 
using an inverted pyramid structure (most important info first). 
Incorporate verified facts from multiple sources, maintaining an objective tone. 
Avoid bullet points, keep language clear, and only reflect confirmed information.";
    }
} 