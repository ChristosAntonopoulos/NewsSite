using NewsSite.Server.Models.PipelineAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsSite.Server.Services.Interfaces
{
    public interface IPipelineService
    {
        Task<IEnumerable<PipelineModel>> GetPipelinesAsync(bool includeDisabled);
        Task<PipelineModel> GetPipelineByIdAsync(string id);
        Task<PipelineModel> CreatePipelineAsync(PipelineModel pipeline);
        Task<bool> UpdatePipelineAsync(PipelineModel pipeline);
        Task<bool> DeletePipelineAsync(string id);
        Task<bool> TogglePipelineStateAsync(string id);
        Task<bool> ReorderPipelineStepsAsync(string id, List<string> stepIds);
    }
} 