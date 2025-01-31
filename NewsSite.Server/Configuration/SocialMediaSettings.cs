namespace NewsSite.Server.Configuration
{
    public class TwitterSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BearerToken { get; set; }
    }

    public class InstagramSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string AccessToken { get; set; }
    }

    public class YouTubeSettings
    {
        public string ApiKey { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
} 