using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsSite.Server.Services.Pipeline.Steps;
using NewsSite.Server.Services.Pipeline.Utils;
using NewsSite.Server.Models.PipelineAggregate;
using System.Linq;
using System.Text.Json;

namespace NewsSite.Server.Services.Pipeline.ArrayProcessing
{
    public class ArrayProcessor
    {
        private readonly IPipelineStep _step;
        private readonly PipelineExecutionContext _context;
        private readonly int _maxRetries;

        public ArrayProcessor(IPipelineStep step, PipelineExecutionContext context, int maxRetries)
        {
            _step = step;
            _context = context;
            _maxRetries = maxRetries;
        }

        public async Task<Dictionary<string, object>> ProcessArrayParameter(
            string parameterKey, 
            string parameterValue, 
            Dictionary<string, object> input,
            Dictionary<string, string> originalParameters)
        {
            _context.AddLog("array-processor", $"Processing array parameter. Key: {parameterKey}, Value: {parameterValue}");
            
            var notation = new ArrayNotation(parameterValue);
            if (!notation.IsArrayNotation)
            {
                throw new ArgumentException("Not an array notation parameter");
            }

            _context.AddLog("array-processor", $"Base path: {notation.BasePath}, Property path: {notation.PropertyPath}");
            _context.AddLog("array-processor", $"Input data: {JsonSerializer.Serialize(input)}");

            var arrayItems = GetArrayItems(input, notation.BasePath);
            var itemsList = arrayItems.ToList().Take(5).ToList();
            _context.AddLog("array-processor", $"Found {itemsList.Count} items to process");

            var results = await ProcessItems(itemsList, parameterKey, notation.PropertyPath, originalParameters);
            _context.AddLog("array-processor", $"Processed {results.Count} items successfully");

            return CreateOutput(results);
        }

        private IEnumerable<object> GetArrayItems(Dictionary<string, object> input, string path)
        {
            if (input == null || string.IsNullOrEmpty(path))
            {
                _context.AddLog("array-processor", $"Input is null or path is empty. Path: {path}");
                return Enumerable.Empty<object>();
            }

            try 
            {
                var pathParts = path.Split('.');
                object current = input;

                foreach (var part in pathParts)
                {
                    if (current is Dictionary<string, object> dict)
                    {
                        if (!dict.TryGetValue(part, out current))
                        {
                            _context.AddLog("array-processor", $"Could not find key {part} in dictionary");
                            return Enumerable.Empty<object>();
                        }
                    }
                    else if (current is IEnumerable<object> list)
                    {
                        return list;
                    }
                    else
                    {
                        _context.AddLog("array-processor", $"Unexpected type at {part}: {current?.GetType().Name}");
                        return Enumerable.Empty<object>();
                    }
                }

                if (current is IEnumerable<object> finalList)
                {
                    return finalList;
                }
                else if (current is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return jsonElement.EnumerateArray().Select(e => e.GetRawText());
                }

                _context.AddLog("array-processor", $"Final value is not an array. Type: {current?.GetType().Name}");
                return Enumerable.Empty<object>();
            }
            catch (Exception ex)
            {
                _context.AddLog("array-processor", $"Error getting array items: {ex.Message}");
                return Enumerable.Empty<object>();
            }
        }

        private async Task<List<ProcessedItem>> ProcessItems(
            IEnumerable<object> items, 
            string parameterKey,
            string propertyPath,
            Dictionary<string, string> originalParameters)
        {
            var results = new List<ProcessedItem>();
            var baseDelayMs = 5000; // 5 seconds base delay
            var currentDelay = baseDelayMs;
            var maxRetries = 3;

            foreach (var item in items)
            {
                var itemValue = ExtractPropertyValue(item, propertyPath);
                if (itemValue != null)
                {
                    var success = false;
                    var retryCount = 0;

                    while (!success && retryCount < maxRetries)
                    {
                        try
                        {
                            // Wait before making the request
                            _context.AddLog("array-processor", $"Waiting {currentDelay}ms before next request...");
                            //await Task.Delay(currentDelay);

                            var parameters = CreateParametersForItem(originalParameters, parameterKey, itemValue);
                            var result = await ExecuteStepForItem(parameters, item);
                            results.Add(result);
                            
                            // Reset delay on success
                            currentDelay = baseDelayMs;
                            success = true;
                        }
                        catch (Exception ex) when (ex.ToString().Contains("429"))
                        {
                            retryCount++;
                            // Exponential backoff: double the delay
                            currentDelay *= 2;
                            _context.AddLog("array-processor", $"Rate limit hit. Retry {retryCount}/{maxRetries} with {currentDelay}ms delay");
                            
                            if (retryCount >= maxRetries)
                            {
                                _context.AddLog("array-processor", $"Max retries reached for item. Skipping.");
                                throw;
                            }
                        }
                    }
                }
            }

            return results;
        }

        private string ExtractPropertyValue(object item, string propertyPath)
        {
            try
            {
                if (item is string jsonString)
                {
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
                    if (jsonElement.TryGetProperty(propertyPath, out var property))
                    {
                        return property.GetString();
                    }
                }
                else if (item is JsonElement jsonElement)
                {
                    if (jsonElement.TryGetProperty(propertyPath, out var property))
                    {
                        return property.GetString();
                    }
                }
                else
                {
                    return item.GetType()
                              .GetProperty(propertyPath)?
                              .GetValue(item)?
                              .ToString();
                }

                _context.AddLog("array-processor", $"Could not extract property {propertyPath} from item");
                return null;
            }
            catch (Exception ex)
            {
                _context.AddLog("array-processor", $"Error extracting property value: {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, string> CreateParametersForItem(
            Dictionary<string, string> originalParameters,
            string parameterKey,
            string itemValue)
        {
            return new Dictionary<string, string>(originalParameters)
            {
                [parameterKey] = itemValue
            };
        }

        private async Task<ProcessedItem> ExecuteStepForItem(
            Dictionary<string, string> parameters,
            object originalItem)
        {
            var input = new Dictionary<string, object> { ["item"] = originalItem };
            var result = await _step.ExecuteAsync(_context, input, parameters);
            
            return new ProcessedItem
            {
                OriginalItem = originalItem,
                ProcessingResult = result,
                ProcessedAt = DateTime.UtcNow
            };
        }

        private Dictionary<string, object> CreateOutput(List<ProcessedItem> results)
        {
            return new Dictionary<string, object>
            {
                ["results"] = results,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["processedItems"] = results.Count,
                    ["timestamp"] = DateTime.UtcNow
                }
            };
        }
    }
} 