using System.Collections.Generic;

namespace NewsSite.Server.Data
{
    public interface IDatabaseOperations
    {
        Task<Dictionary<string, object>> ExecuteQueryAsync(string query);
        Task<Dictionary<string, object>> ExecuteInsertAsync(string collection, Dictionary<string, object> data);
        Task<Dictionary<string, object>> ExecuteUpdateAsync(string collection, string filter, Dictionary<string, object> data);
        Task<Dictionary<string, object>> ExecuteDeleteAsync(string collection, string filter);
    }
} 