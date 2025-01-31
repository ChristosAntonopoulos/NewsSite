using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NewsSite.Server.Models.PipelineAggregate
{
    public class PipelineExecutionContext
    {
        public string PipelineId { get; }
        public string ExecutionId { get; }
        public Dictionary<string, Dictionary<string, object>> StepOutputs { get; }
        public Dictionary<string, object> GlobalContext { get; }
        public DateTime StartTime { get; }
        public DateTime? EndTime { get; private set; }
        public PipelineExecutionStatus Status { get; private set; }
        public List<PipelineExecutionLog> Logs { get; }
        public Dictionary<string, StepExecutionState> StepStates { get; }
        public int TotalSteps { get; }
        public int CompletedSteps { get; private set; }
        public List<string> ExecutionPath { get; }
        public Dictionary<string, int> RetryCount { get; }
        private Dictionary<string, StepDataFlow> StepDataFlows { get; }

        public PipelineExecutionContext(string pipelineId, int totalSteps)
        {
            PipelineId = pipelineId;
            ExecutionId = Guid.NewGuid().ToString();
            StepOutputs = new Dictionary<string, Dictionary<string, object>>();
            GlobalContext = new Dictionary<string, object>();
            StartTime = DateTime.UtcNow;
            Status = PipelineExecutionStatus.Running;
            Logs = new List<PipelineExecutionLog>();
            StepStates = new Dictionary<string, StepExecutionState>();
            TotalSteps = totalSteps;
            CompletedSteps = 0;
            ExecutionPath = new List<string>();
            RetryCount = new Dictionary<string, int>();
            StepDataFlows = new Dictionary<string, StepDataFlow>();
        }

        public Dictionary<string, object> GetExecutionState()
        {
            var result = new Dictionary<string, object>
            {
                ["pipelineId"] = PipelineId,
                ["executionId"] = ExecutionId,
                ["status"] = Status.ToString(),
                ["startTime"] = StartTime,
                ["endTime"] = EndTime,
                ["totalSteps"] = TotalSteps,
                ["completedSteps"] = CompletedSteps,
                ["logs"] = Logs,
                ["stepStates"] = StepStates.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                ),
                ["executionPath"] = ExecutionPath
            };

            // Add step outputs
            var outputs = new Dictionary<string, object>();
            foreach (var kvp in StepOutputs)
            {
                outputs[kvp.Key] = new Dictionary<string, object>
                {
                    ["output"] = kvp.Value,
                    ["state"] = StepStates.GetValueOrDefault(kvp.Key, StepExecutionState.Pending).ToString(),
                    ["retryCount"] = RetryCount.GetValueOrDefault(kvp.Key, 0),
                    ["dataFlow"] = StepDataFlows.TryGetValue(kvp.Key, out var flow) 
                        ? flow
                        : null
                };
            }

            result["stepOutputs"] = outputs;
            result["globalContext"] = GlobalContext;

            return result;
        }

        public void AddStepOutput(string stepId, Dictionary<string, object> output)
        {
            StepOutputs[stepId] = output;
            UpdateStepState(stepId, StepExecutionState.Completed);
            CompletedSteps++;
        }

        public void TrackDataFlow(string stepId, string inputPath, string outputPath, bool isArray = false)
        {
            if (!StepDataFlows.ContainsKey(stepId))
            {
                StepDataFlows[stepId] = new StepDataFlow 
                { 
                    StepId = stepId,
                    InputPaths = new List<DataPath>(),
                    OutputPaths = new List<DataPath>() 
                };
            }

            var flow = StepDataFlows[stepId];

            // Track input path
            if (!string.IsNullOrEmpty(inputPath))
            {
                flow.InputPaths.Add(new DataPath 
                { 
                    Path = inputPath,
                    IsArray = isArray,
                    Value = GetValueFromPath<object>(inputPath)
                });
            }

            // Track output path
            if (!string.IsNullOrEmpty(outputPath))
            {
                flow.OutputPaths.Add(new DataPath 
                { 
                    Path = outputPath,
                    IsArray = isArray,
                    Value = null // Will be set when output is added
                });
            }
        }

        public Dictionary<string, object> GetStepOutput(string stepId)
        {
            return StepOutputs.TryGetValue(stepId, out var output) ? output : null;
        }

        public void AddLog(string stepId, string message, LogLevel level = LogLevel.Information)
        {
            Logs.Add(new PipelineExecutionLog
            {
                Timestamp = DateTime.UtcNow,
                StepId = stepId,
                Message = message,
                Level = level
            });
        }

        public void UpdateStepState(string stepId, StepExecutionState state)
        {
            StepStates[stepId] = state;
        }

        public bool ShouldRetry(string stepId, int maxRetries)
        {
            if (!RetryCount.ContainsKey(stepId))
            {
                RetryCount[stepId] = 0;
            }

            return RetryCount[stepId]++ < maxRetries;
        }

        public void AddToExecutionPath(string stepId)
        {
            ExecutionPath.Add(stepId);
        }

        public bool EvaluateCondition(string condition)
        {
            try
            {
                var parts = condition.Split(new[] { "==", "!=", ">", "<", ">=", "<=" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                var leftValue = ResolveValue(parts[0].Trim());
                var rightValue = ResolveValue(parts[1].Trim());
                var op = condition.Substring(parts[0].Length, condition.Length - parts[0].Length - parts[1].Length).Trim();

                return op switch
                {
                    "==" => leftValue == rightValue,
                    "!=" => leftValue != rightValue,
                    ">" => Convert.ToDouble(leftValue) > Convert.ToDouble(rightValue),
                    "<" => Convert.ToDouble(leftValue) < Convert.ToDouble(rightValue),
                    ">=" => Convert.ToDouble(leftValue) >= Convert.ToDouble(rightValue),
                    "<=" => Convert.ToDouble(leftValue) <= Convert.ToDouble(rightValue),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private string ResolveValue(string value)
        {
            if (value.StartsWith("${") && value.EndsWith("}"))
            {
                var path = value.Substring(2, value.Length - 3);
                if (path.StartsWith("context."))
                {
                    return GetValueFromPath<string>(path.Substring(8));
                }
                else if (path.StartsWith("step."))
                {
                    var parts = path.Substring(5).Split('.');
                    var stepOutput = GetStepOutput(parts[0]);
                    return stepOutput?.GetValueOrDefault(parts[1])?.ToString() ?? string.Empty;
                }
            }
            return value.Trim('"');
        }

        public T GetValueFromPath<T>(string path, T defaultValue = default)
        {
            try
            {
                var parts = path.Split('.');
                object current = GlobalContext;

                foreach (var part in parts)
                {
                    if (current is Dictionary<string, object> dict)
                    {
                        if (!dict.TryGetValue(part, out current))
                        {
                            return defaultValue;
                        }
                    }
                    else if (current is List<object> list)
                    {
                        if (int.TryParse(part, out var index) && index >= 0 && index < list.Count)
                        {
                            current = list[index];
                        }
                        else
                        {
                            return defaultValue;
                        }
                    }
                    else
                    {
                        return defaultValue;
                    }
                }

                if (current is T result)
                {
                    return result;
                }

                try
                {
                    return (T)Convert.ChangeType(current, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetValueAtPath(string path, Dictionary<string, object> value)
        {
            if (value != null)
            {
                GlobalContext[path] = value;

                // Update data flow output value if tracked
                foreach (var flow in StepDataFlows.Values)
                {
                    foreach (var outputPath in flow.OutputPaths)
                    {
                        if (outputPath.Path == path)
                        {
                            outputPath.Value = value;
                        }
                    }
                }
            }
        }
    }

    public class StepDataFlow
    {
        public string StepId { get; set; }
        public List<DataPath> InputPaths { get; set; }
        public List<DataPath> OutputPaths { get; set; }
    }

    public class DataPath
    {
        public string Path { get; set; }
        public bool IsArray { get; set; }
        public object Value { get; set; }
    }

    public enum PipelineExecutionStatus
    {
        Running,
        Completed,
        Failed,
        Cancelled
    }

    public enum StepExecutionState
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped,
        Cancelled
    }

    public class PipelineExecutionLog
    {
        public DateTime Timestamp { get; set; }
        public string StepId { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }
    }
} 