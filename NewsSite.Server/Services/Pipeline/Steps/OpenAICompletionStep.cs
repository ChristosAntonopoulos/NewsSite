using System.Text.Json;
using Microsoft.Extensions.Options;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models.PipelineAggregate;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.Interfaces;
using Microsoft.Extensions.Logging;
using OpenAI.ObjectModels;
using NewsSite.Server.Services.Pipeline.Utils;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class OpenAICompletionStep : BaseStep
    {
        private readonly IOpenAIService _openAI;
        private readonly OpenAISettings _settings;

        public override string StepType => "openai.completion";

        public OpenAICompletionStep(
            IOpenAIService openAIService,
            IOptions<OpenAISettings> settings, 
            ILogger<OpenAICompletionStep> logger) : base(logger)
        {
            _openAI = openAIService;
            _settings = settings.Value;
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(PipelineExecutionContext context, Dictionary<string, object> input, Dictionary<string, string> parameters)
        {
            try
            {
                ValidateRequiredParameter("query", parameters, context.ExecutionId);
                ValidateRequiredParameter("outputPath", parameters, context.ExecutionId);

                var prompt = ReplaceVariables(parameters["query"], context, input);
                var maxTokens = GetTypedParameter(parameters, "maxTokens", 150);
                var temperature = GetTypedParameter(parameters, "temperature", 0.7f);
                // Using gpt-3.5-turbo-instruct as the default model since text-davinci-003 is deprecated
                var model = GetTypedParameter(parameters, "model", "gpt-3.5-turbo-instruct");

                var completionRequest = new CompletionCreateRequest
                {
                    Prompt = prompt,
                    MaxTokens = maxTokens,
                    Temperature = temperature,
                    Model = model
                };

                var completionResult = await _openAI.Completions.CreateCompletion(completionRequest);
                if (!completionResult.Successful)
                {
                    throw new StepExecutionException(context.ExecutionId, 
                        $"OpenAI API error: {completionResult.Error?.Message}");
                }

                var result = new Dictionary<string, object>
                {
                    { "text", completionResult.Choices[0].Text },
                    { "usage", new Dictionary<string, object>
                        {
                            { "prompt_tokens", completionResult.Usage.PromptTokens },
                            { "completion_tokens", completionResult.Usage.CompletionTokens },
                            { "total_tokens", completionResult.Usage.TotalTokens }
                        }
                    }
                };

                SetOutputValue(context, parameters, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing OpenAI completion step");
                throw new StepExecutionException(context.ExecutionId, "OpenAI completion failed", ex);
            }
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "prompt", "outputPath" };
        }
    }
} 