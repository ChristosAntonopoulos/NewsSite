using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json;

namespace NewsSite.Server.Models.BaseAggregate
{
    public class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } =  ObjectId.GenerateNewId().ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


    }
}
