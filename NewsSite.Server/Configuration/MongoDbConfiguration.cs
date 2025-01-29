using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Configuration
{
    public static class MongoDbConfiguration
    {
        public static void Configure()
        {
            // Create a convention pack for handling dictionaries and complex types
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreIfNullConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            };

            // Register the convention pack
            ConventionRegistry.Register(
                "NewsSiteConventions",
                pack,
                t => true);

            // Register class maps
            if (!BsonClassMap.IsClassMapRegistered(typeof(PipelineStep)))
            {
                BsonClassMap.RegisterClassMap<PipelineStep>(cm =>
                {
                    cm.AutoMap();
                    cm.MapMember(c => c.Parameters).SetElementName("configuration");
                    cm.SetIgnoreExtraElements(true);
                });
            }

            // Register any custom serializers if needed
            if (!BsonClassMap.IsClassMapRegistered(typeof(Dictionary<string, object>)))
            {
                BsonClassMap.RegisterClassMap<Dictionary<string, object>>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }
    }
} 