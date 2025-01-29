using System.Collections.Generic;
using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Services.Pipeline
{
    public interface IPipelineExecutionService
    {
        Task<Dictionary<string, object>> ExecutePipelineAsync(string pipelineId, Dictionary<string, object> initialInput = null);
    }
} 