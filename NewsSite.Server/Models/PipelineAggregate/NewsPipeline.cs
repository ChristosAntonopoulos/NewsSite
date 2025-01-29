using NewsSite.Server.Models;
using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Models
{
    public class NewsPipeline : PipelineModel
    {
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public List<ProcessedArticle> ProcessedArticles { get; set; } = new();
        public List<PipelineExecution> Executions { get; set; } = new();
    }
}
