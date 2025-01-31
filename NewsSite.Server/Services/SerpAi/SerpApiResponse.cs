using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class SerpApiResponse
{
    [JsonPropertyName("news_results")]
    public List<SerpApiNewsResult> NewsResults { get; set; } = new();

    [JsonPropertyName("search_metadata")]
    public SearchMetadata? SearchMetadata { get; set; }
}

public class SerpApiNewsResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}

public class SearchMetadata
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("processed_at")]
    public string ProcessedAt { get; set; } = string.Empty;

    [JsonPropertyName("total_time_taken")]
    public double TotalTimeTaken { get; set; }
} 