using System.Text.Json;
using Microsoft.Extensions.Logging;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.Pipeline.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsSite.Server.Services.Pipeline.DataTransformations;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public abstract class BaseStep : IPipelineStep
    {
        protected readonly ILogger<BaseStep> _logger;
        protected List<TransformationConfig> Transformations { get; set; } = new();

        public abstract string StepType { get; }

        protected BaseStep(ILogger<BaseStep> logger)
        {
            _logger = logger;
        }

        public abstract Task<Dictionary<string, object>> ExecuteAsync(PipelineExecutionContext context, Dictionary<string, object> input, Dictionary<string, string> parameters);

        public virtual Task<bool> ValidateAsync(Dictionary<string, string> parameters)
        {
            var requiredParams = GetRequiredParameters();
            return Task.FromResult(requiredParams.All(p => parameters.ContainsKey(p)));
        }

        public abstract IEnumerable<string> GetRequiredParameters();

        protected string ReplaceVariables(string template, PipelineExecutionContext context, Dictionary<string, object> input)
        {
            return VariableReplacer.ReplaceVariables(template, context, input);
        }

        protected void ValidateRequiredParameter(string paramName, Dictionary<string, string> parameters, string executionId)
        {
            if (!parameters.TryGetValue(paramName, out _))
                throw new StepExecutionException(executionId, $"{paramName} parameter is required");
        }

        protected T GetTypedParameter<T>(Dictionary<string, string> parameters, string key, T defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        protected void SetOutputValue(PipelineExecutionContext context, Dictionary<string, string> parameters, object value)
        {
            if (parameters.TryGetValue("outputPath", out var outputPath))
            {
                var outputDict = new Dictionary<string, object>();
                if (value is Dictionary<string, object> dict)
                {
                    outputDict = dict;
                }
                else
                {
                    outputDict["result"] = value;
                }
                context.SetValueAtPath(outputPath, outputDict);
            }
        }

        protected Dictionary<string, object> ApplyTransformations(Dictionary<string, object> data)
        {
            var result = new Dictionary<string, object>(data);

            foreach (var config in Transformations)
            {
                if (!data.ContainsKey(config.Field)) continue;

                var value = data[config.Field];
                var outputField = string.IsNullOrEmpty(config.OutputField) ? config.Field : config.OutputField;

                if (config.ApplyToList && value is IEnumerable<object> list)
                {
                    var transformedList = new List<object>();
                    foreach (var item in list)
                    {
                        var transformed = DataTransformer.Transform(item, config.Transformation);
                        if (transformed != null)
                        {
                            transformedList.Add(transformed);
                        }
                    }
                    result[outputField] = transformedList;
                }
                else
                {
                    var transformed = DataTransformer.Transform(value, config.Transformation);
                    if (transformed != null)
                    {
                        result[outputField] = transformed;
                    }
                }
            }

            return result;
        }

        public void AddTransformation(TransformationConfig config)
        {
            Transformations.Add(config);
        }

        public void ClearTransformations()
        {
            Transformations.Clear();
        }
    }
} 