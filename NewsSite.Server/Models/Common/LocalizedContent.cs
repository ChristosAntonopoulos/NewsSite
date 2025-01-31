using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Server.Models.Common
{
    public class LocalizedContent
    {
        [Required]
        [MaxLength(200)]
        [BsonElement("en")]
        public string En { get; set; }

        [Required]
        [MaxLength(200)]
        [BsonElement("el")]
        public string El { get; set; }

        public string GetContent(string languageCode)
        {
            return languageCode.ToLower() switch
            {
                "el" => El ?? En, // Fallback to English if Greek is not available
                _ => En ?? El     // Fallback to Greek if English is not available
            };
        }
    }
} 