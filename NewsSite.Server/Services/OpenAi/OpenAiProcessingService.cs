using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using NewsSite.Server.Services.Pipeline;
using NewsSite.Server.Models.PromtingSettings;
using NewsSite.Server.Models.ChatSettings;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace NewsSite.Server.Services.OpenAi
{
    public class OpenAiProcessingService : IStep
    {
        private readonly OpenAIClient _openAiClient;
        private readonly ILogger<OpenAiProcessingService> _logger;
        private readonly OpenAiChatSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public string Name => "OpenAI Processing";

        public OpenAiProcessingService(
            ILogger<OpenAiProcessingService> logger,
            IConfiguration configuration,
            HttpClient httpClient,
            string apiKey,
            OpenAiChatSettings settings,
            string? endpoint = null)
        {
            _logger = logger;
            _settings = settings;
            _configuration = configuration;
            _httpClient = httpClient;

            // Create client (support both Azure OpenAI and OpenAI)
            _openAiClient = string.IsNullOrEmpty(endpoint)
                ? new OpenAIClient(apiKey)
                : new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public async Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs)
        {
            try
            {
                _logger.LogInformation("Starting OpenAI processing step");

                var formattedPrompt = FormatPrompt(_settings.UserPrompt, inputs);
                var response = await SendOpenAIRequest(formattedPrompt);

                return new Dictionary<string, object>
                {
                    { "result", response.Content },
                    { "model", _settings.Model },
                    { "tokens_used", response.TotalTokens },
                    { "finish_reason", response.FinishReason }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OpenAI processing");
                throw;
            }
        }

        private string FormatPrompt(string template, Dictionary<string, object> inputs)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var result = template;
            foreach (var input in inputs)
            {
                result = result.Replace($"{{{input.Key}}}", input.Value?.ToString() ?? "");
            }
            return result;
        }

        private async Task<(string Content, int TotalTokens, string FinishReason)> SendOpenAIRequest(string prompt)
        {
            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens,
                DeploymentName = _settings.Model
            };

            if (!string.IsNullOrEmpty(_settings.SystemPrompt))
            {
                chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(_settings.SystemPrompt));
            }
            chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(prompt));

            var response = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
            var choice = response.Value.Choices[0];

            return (
                Content: choice.Message.Content,
                TotalTokens: response.Value.Usage.TotalTokens,
                FinishReason: choice.FinishReason?.ToString() ?? "unknown"
            );
        }
    }
} 