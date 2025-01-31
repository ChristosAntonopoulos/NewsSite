using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json;
using NewsSite.Server.Models.BaseAggregate;

namespace NewsSite.Server.Models.PipelineAggregate
{
    public class PipelineModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public PipelineMode Mode { get; set; }
        public List<PipelineStep> Steps { get; set; } = new();
        public string Description { get; set; }
        public Dictionary<string, string> InputSchema { get; set; }
        public Dictionary<string, string> OutputSchema { get; set; }
        public RetryPolicy RetryPolicy { get; set; }
        public TimeSpan? Timeout { get; set; }
    }


   
} 