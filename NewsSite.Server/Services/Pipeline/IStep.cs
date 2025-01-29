namespace NewsSite.Server.Services.Pipeline
{
    public interface IStep
    {
        string Name { get; }
        Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs);
    }
} 