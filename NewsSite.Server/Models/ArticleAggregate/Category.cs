using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NewsSite.Server.Models.BaseAggregate;
using NewsSite.Server.Models.Common;

namespace NewsSite.Server.Models.ArticleAggregate
{
    [BsonIgnoreExtraElements]
    public class Category : BaseModel
    {
        [BsonElement("Name")]
        public LocalizedContent Name { get; set; } = new();

        [BsonElement("Description")]
        public LocalizedContent Description { get; set; } = new();

        [BsonElement("Slug")]
        public Dictionary<string, string> Slug { get; set; } = new();

        [BsonElement("Topics")]
        public List<Topic> Topics { get; set; } = new();

        [BsonElement("Articles")]
        public List<Article> Articles { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class Topic : BaseModel
    {
        [BsonElement("Name")]
        public LocalizedContent Name { get; set; } = new();

        [BsonElement("Description")]
        public LocalizedContent Description { get; set; } = new();

        [BsonElement("Slug")]
        public Dictionary<string, string> Slug { get; set; } = new();

        [BsonElement("CategoryId")]
        public string CategoryId { get; set; }

        [BsonElement("Category")]
        public Category Category { get; set; }

        [BsonElement("Articles")]
        public List<Article> Articles { get; set; } = new();
    }
} 