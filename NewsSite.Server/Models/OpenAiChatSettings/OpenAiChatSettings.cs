using NewsSite.Server.Models.BaseAggregate;
using System.Text.Json;

namespace NewsSite.Server.Models.PromtingSettings
{
    public class OpenAiChatSettings : BaseModel
    {
        public string SystemPrompt { get; set; }
        public string UserPrompt { get; set; }
        public string ConversationName { get; set; }
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
        public string Model { get; set; } = "gpt-4"; //TO-DO
        public Dictionary<string, string> Parameters { get; set; }
        public OpenAiChatSettings() : base()
        {
                
        }
    }
}
