using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.Pipeline.Steps;

namespace NewsSite.Server.Services.Pipeline
{
    public interface IStepFactory
    {
        IPipelineStep CreateStep(string stepType);
        T ParseConfiguration<T>(Dictionary<string, string> parameters) where T : StepConfiguration;
    }

    public class StepFactory : IStepFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _stepTypes;

        public StepFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _stepTypes = new Dictionary<string, Type>
            {
                { "openai.completion", typeof(OpenAICompletionStep) },
                { "serp.search", typeof(SerpApiStep) },
                { "database.operation", typeof(DatabaseStep) },
                { "news.category", typeof(NewsCategoryStep) }
            };
        }

        public IPipelineStep CreateStep(string stepType)
        {
            if (!_stepTypes.TryGetValue(stepType, out var type))
            {
                throw new InvalidOperationException($"Unknown step type: {stepType}");
            }

            var step = ActivatorUtilities.CreateInstance(_serviceProvider, type) as IPipelineStep;
            if (step == null)
            {
                throw new InvalidOperationException($"Failed to create step instance for type: {stepType}");
            }

            return step;
        }

        public T ParseConfiguration<T>(Dictionary<string, string> parameters) where T : StepConfiguration
        {
            var jsonString = JsonSerializer.Serialize(parameters);
            return JsonSerializer.Deserialize<T>(jsonString) ?? 
                throw new InvalidOperationException($"Failed to parse configuration for type {typeof(T).Name}");
        }
    }
} 