using NewsSite.Server.Models.BaseAggregate;
using NewsSite.Server.Models.PromtingSettings;

namespace NewsSite.Server.Models.ProcessingStepAggregate
{
    public class OpenAiProcessingStep : BaseModel, IProcessingStep
    {
        // Basic properties
        public string PipelineId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }
        
        // OpenAI specific settings
        public OpenAiChatSettings? OpenAiChatSettings { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public RetryPolicy RetryPolicy { get; set; }

        // Required for EF Core

        public OpenAiProcessingStep(string pipelineId, string name, string description, int order)
        {
            PipelineId = pipelineId;
            Name = name;
            Description = description;
            Order = order;
            IsEnabled = true;
            OpenAiChatSettings = new OpenAiChatSettings();
            RetryPolicy = new RetryPolicy { MaxRetries = 3, InitialDelay = TimeSpan.FromSeconds(1) };
        }

        public Task<ProcessingResult> ProcessAsync(Dictionary<string, object> inputs)
        {
            // This will be implemented in the service layer
            throw new NotImplementedException();
        }
    }
}
