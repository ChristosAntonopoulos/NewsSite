using NewsSite.Server.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

public interface IMongoDbConnectionService
{
    Task<IMongoDatabase> GetDatabaseAsync();
    IMongoCollection<T> GetCollection<T>(string name);
}

public class MongoDbConnectionService : IMongoDbConnectionService
{
    private readonly MongoDbSettings _settings;
    private readonly ILogger<MongoDbConnectionService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private IMongoDatabase? _database;

    public MongoDbConnectionService(
        IOptions<MongoDbSettings> settings,
        ILogger<MongoDbConnectionService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings), "MongoDB settings cannot be null");
        if (string.IsNullOrEmpty(_settings.ConnectionString))
            throw new ArgumentException("MongoDB connection string cannot be empty", nameof(settings));
        if (string.IsNullOrEmpty(_settings.DatabaseName)) 
            throw new ArgumentException("MongoDB database name cannot be empty", nameof(settings));
        _logger = logger;
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<IMongoDatabase> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var client = new MongoClient(CreateMongoClientSettings());
            _database = client.GetDatabase(_settings.DatabaseName);
            
            // Verify connection
            await _database.RunCommandAsync((Command<object>)"{ping:1}");
            return _database;
        });
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        if (_database == null)
            throw new InvalidOperationException("Database not initialized. Call GetDatabaseAsync first.");
        return _database.GetCollection<T>(name);
    }

    private MongoClientSettings CreateMongoClientSettings()
    {
        var settings = MongoClientSettings.FromConnectionString(_settings.ConnectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeout);
        settings.ConnectTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeout);
        return settings;
    }

    private AsyncRetryPolicy CreateRetryPolicy()
    {
        return Policy
            .Handle<MongoException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                _settings.RetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(_settings.RetryWaitTimeSeconds),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Failed to connect to MongoDB. Retry attempt {RetryCount} of {MaxRetries}. Waiting {WaitTime} seconds.",
                        retryCount,
                        _settings.RetryAttempts,
                        timeSpan.TotalSeconds);
                });
    }
} 