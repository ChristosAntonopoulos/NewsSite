using System.Text.Json.Serialization;

namespace NewsSite.Server.Models.PipelineAggregate
{
    public abstract class StepConfiguration
    {
        public string OutputPath { get; set; }
    }

    public class OpenAIStepConfiguration : StepConfiguration
    {
        public string Prompt { get; set; }
        public int MaxTokens { get; set; } = 150;
        public float Temperature { get; set; } = 0.7f;
        public string Model { get; set; } = "text-davinci-003";
    }

    public class SerpApiStepConfiguration : StepConfiguration
    {
        public string Query { get; set; }
        public string Engine { get; set; } = "google";
        public int ResultCount { get; set; } = 10;
    }

    public class DatabaseStepConfiguration : StepConfiguration
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DatabaseOperation Operation { get; set; }
        public string Collection { get; set; }
        public string Query { get; set; }
        public string Filter { get; set; }
    }

    public class TwitterStepConfiguration : StepConfiguration
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TwitterOperation Operation { get; set; }
        public string Query { get; set; }
        public int MaxResults { get; set; } = 100;
        public bool IncludeRetweets { get; set; } = false;
        public string Language { get; set; } = "en";
        public string SinceId { get; set; }
        public string UntilId { get; set; }
    }

    public class InstagramStepConfiguration : StepConfiguration
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InstagramOperation Operation { get; set; }
        public string Username { get; set; }
        public string Hashtag { get; set; }
        public int MaxResults { get; set; } = 50;
        public bool IncludeStories { get; set; } = false;
        public string SinceTimestamp { get; set; }
    }

    public class YouTubeStepConfiguration : StepConfiguration
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public YouTubeOperation Operation { get; set; }
        public string Query { get; set; }
        public string ChannelId { get; set; }
        public int MaxResults { get; set; } = 50;
        public string Order { get; set; } = "date";
        public string PublishedAfter { get; set; }
        public string Type { get; set; } = "video";
    }

    public enum DatabaseOperation
    {
        Query,
        Insert,
        Update,
        Delete
    }

    public enum TwitterOperation
    {
        Search,
        UserTimeline,
        Mentions,
        Trends
    }

    public enum InstagramOperation
    {
        UserPosts,
        HashtagPosts,
        UserStories,
        Explore
    }

    public enum YouTubeOperation
    {
        Search,
        ChannelVideos,
        Trending,
        Comments
    }
} 