public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public bool ExponentialBackoff { get; set; } = true;
    public List<Type> RetryableExceptions { get; set; } = new();
} 