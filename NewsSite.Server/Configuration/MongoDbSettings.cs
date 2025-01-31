namespace NewsSite.Server.Configuration
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public CollectionNames CollectionNames { get; set; } = new();
        public int ConnectionTimeout { get; set; } = 30;
        public int RetryAttempts { get; set; } = 3;
        public int RetryWaitTimeSeconds { get; set; } = 5;
    }

    public class CollectionNames
    {
        public string ProcessedArticles { get; set; } = string.Empty;
        public string Pipelines { get; set; } = string.Empty;
        public string ProcessingLogs { get; set; } = string.Empty;
        public string PipelineExecutions { get; set; } = string.Empty;
    }
} 