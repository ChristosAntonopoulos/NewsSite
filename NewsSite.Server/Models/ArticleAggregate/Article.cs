using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NewsSite.Server.Models.BaseAggregate;
using NewsSite.Server.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Server.Models.ArticleAggregate
{
    [BsonIgnoreExtraElements]
    public class Article : BaseModel
    {      
        [Required]
        [BsonElement("Title")]
        public LocalizedContent Title { get; set; } = new();

        [Required]
        [BsonElement("Summary")]
        public LocalizedContent Summary { get; set; } = new();

        [BsonElement("VerifiedFacts")]
        public List<VerifiedFact> VerifiedFacts { get; set; } = new();

        [BsonElement("SourceCount")]
        public int SourceCount { get; set; }

        [BsonElement("Sources")]
        public List<ArticleSource> Sources { get; set; } = new();

        [Required]
        [BsonElement("FeaturedImages")]
        public List<string> FeaturedImages { get; set; } = new();

        [BsonElement("Version")]
        public int Version { get; set; }

        [BsonElement("WordCount")]
        public Dictionary<string, int> WordCount { get; set; } = new();

        [BsonElement("Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [Required]
        [BsonElement("Content")]
        public LocalizedContent Content { get; set; } = new();

        [BsonElement("Url")]
        public Dictionary<string, string> Url { get; set; } = new();

        [BsonElement("PublishedAt")]
        public DateTime PublishedAt { get; set; }

        [Required]
        [BsonElement("CategoryId")]
        public string CategoryId { get; set; }

        [Required]
        [BsonElement("Category")]
        public Category Category { get; set; }
        
        [Required]
        [BsonElement("TopicId")]
        public string TopicId { get; set; }

        [Required]
        [BsonElement("Topic")]
        public Topic Topic { get; set; }

        [BsonElement("Analysis")]
        public string Analysis { get; set; }

        [BsonElement("ProcessedAt")]
        public DateTime ProcessedAt { get; set; }

        //public virtual ArticleSentiment Sentiment { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ArticleSource
    {
        [BsonElement("Title")]
        public LocalizedContent Title { get; set; } = new();

        [BsonElement("Url")]
        public string Url { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class VerifiedFact
    {
        [BsonElement("Fact")]
        public LocalizedContent Fact { get; set; } = new();

        [BsonElement("Confidence")]
        public int Confidence { get; set; }

        [BsonElement("Sources")]
        public List<ArticleSource> Sources { get; set; } = new();

        [BsonElement("Attribution")]
        public LocalizedContent Attribution { get; set; } = new();
    }
}