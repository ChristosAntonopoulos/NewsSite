using System;
using NewsSite.Server.Models.ArticleAggregate;

namespace NewsSite.Server.Models
{
    public class ProcessedArticle : Article
    {
        public int PipelineId { get; set; }
        public string Analysis { get; set; }
        public DateTime ProcessedAt { get; set; }
        public NewsPipeline Pipeline { get; set; }
    }
} 