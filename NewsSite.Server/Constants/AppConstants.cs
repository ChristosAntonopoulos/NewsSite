namespace NewsSite.Server.Constants
{
    public static class AppConstants
    {
        public static class Cache
        {
            public const int DefaultExpirationHours = 1;
            public const int ArticleExpirationDays = 1;
            public const string ArticlePrefix = "article_";
            public const string TopicPrefix = "topic_";
        }

        public static class Pipeline
        {
            public const int MaxRetries = 3;
            public const int DefaultBatchSize = 10;
            public const int MaxConcurrentPipelines = 5;
        }

        public static class RateLimits
        {
            public const int RequestsPerSecond = 10;
            public const int BurstSize = 20;
        }
    }
} 