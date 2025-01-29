using System.Threading.Tasks;
using System.Collections.Generic;
using NewsSite.Server.Models.Pipeline;

namespace NewsSite.Server.Services.SerpAi
{
    public interface ISerpApiService
    {
        Task<Dictionary<string, object>> SearchAsync(string query, SearchParameters parameters = null);
    }
} 