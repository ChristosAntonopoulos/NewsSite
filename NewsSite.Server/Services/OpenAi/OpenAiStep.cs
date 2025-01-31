using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NewsSite.Server.Models;
using NewsSite.Server.Models.PromtingSettings;
using NewsSite.Server.Models.ChatSettings;
using NewsSite.Server.Data;

namespace NewsSite.Server.Services.OpenAi
{
    public class OpenAiStep : IProcessingStep
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAiStep> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IApplicationDbContext _dbContext;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "OpenAI Processing Step";
        public OpenAiChatSettings? ChatSettings { get; set; }
        public string Settings { get; set; } = "{}";
        public StepType StepType { get; set; }
        public RetryPolicy RetryPolicy { get; set; } = new();
        public TimeSpan Timeout { get; set; }
        public string PipelineId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsEnabled { get; set; }

        public OpenAiStep(
            HttpClient httpClient,
            ILogger<OpenAiStep> logger,
            IApplicationDbContext dbContext,
            string apiKey)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            StepType = StepType.AIProcessing;
            Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ProcessingResult> ExecuteAsync(Dictionary<string, object> inputs)
        {
            try
            {
                _logger.LogInformation("Starting OpenAI step execution for {StepId}", Id);

                var settings = GetSettings();
                var chatSettings = await LoadChatSettingsAsync(inputs);

                // Execute with retry policy
                return await ExecuteWithRetryAsync(async () =>
                {
                    var response = await SendOpenAIRequest(chatSettings);

                    if (response.Choices.Any())
                    {
                        return ProcessingResult.Complete(
                            Id,
                            response.Choices[0].Content,
                            new Dictionary<string, object>
                            {
                                { "model", settings.Model },
                                { "tokens_used", response.TotalTokensUsed },
                                { "prompt_tokens", response.PromptTokens }
                            });
                    }

                    return ProcessingResult.Failure(Id, "No response generated");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing OpenAI step {StepId}", Id);
                return ProcessingResult.Failure(Id, ex.Message);
            }
        }

        private async Task<ProcessingResult> ExecuteWithRetryAsync(Func<Task<ProcessingResult>> action)
        {
            var attempts = 0;
            while (attempts < RetryPolicy.MaxRetries)
            {
                try
                {
                    using var cts = new CancellationTokenSource(Timeout);
                    return await action();
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts >= RetryPolicy.MaxRetries)
                    {
                        return ProcessingResult.Failure(Id, $"Max retries exceeded: {ex.Message}");
                    }

                    _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying...", attempts);
                    await Task.Delay(TimeSpan.FromSeconds(RetryPolicy.DelaySeconds * attempts));
                }
            }

            return ProcessingResult.Failure(Id, "Max retries exceeded");
        }

        private OpenAISettings GetSettings() =>
            JsonSerializer.Deserialize<OpenAISettings>(Settings, _jsonOptions) ?? new OpenAISettings();

        private async Task<OpenAiChatSettings> LoadChatSettingsAsync(Dictionary<string, object> inputs)
        {
            var settings = ChatSettings ?? new OpenAiChatSettings();

            // Format prompts with inputs
            settings.UserPrompt = FormatPrompt(settings.UserPrompt, settings.Parameters);
            settings.SystemPrompt = FormatPrompt(settings.SystemPrompt, settings.Parameters);

            // Merge additional inputs
            foreach (var input in inputs)
            {
                if (!settings.Parameters.ContainsKey(input.Key))
                {
                    settings.Parameters[input.Key] = input.Value?.ToString() ?? "";
                }
            }

            return settings;
        }

        private string FormatPrompt(string template, Dictionary<string, string> inputs)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            foreach (var input in inputs)
            {
                template = template.Replace($"{{{input.Key}}}", input.Value);
            }
            return template;
        }

        private async Task<OpenAiChatResponse> SendOpenAIRequest(OpenAiChatSettings chatSettings)
        {
            try
            {
                var requestBody = new
                {
                    model = chatSettings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = chatSettings.SystemPrompt },
                        new { role = "user", content = chatSettings.UserPrompt }
                    },
                    temperature = chatSettings.Temperature,
                    max_tokens = chatSettings.MaxTokens
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();

                var chatResponse = new OpenAiChatResponse
                {
                    TotalTokensUsed = result.GetProperty("usage").GetProperty("total_tokens").GetInt32(),
                    PromptTokens = result.GetProperty("usage").GetProperty("prompt_tokens").GetInt32()
                };

                var choices = result.GetProperty("choices");
                foreach (var choice in choices.EnumerateArray())
                {
                    chatResponse.Choices.Add(new OpenAiChatResponse.ChatChoice
                    {
                        Role = choice.GetProperty("message").GetProperty("role").GetString() ?? "",
                        Content = choice.GetProperty("message").GetProperty("content").GetString() ?? ""
                    });
                }

                return chatResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OpenAI request");
                throw;
            }
        }

        public Task<ProcessingResult> ProcessAsync(Dictionary<string, object> inputs)
        {
            throw new NotImplementedException();
        }
    }
}