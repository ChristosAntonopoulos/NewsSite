using System.Text.Json;
using System.Text.RegularExpressions;
using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Services.Pipeline.Utils
{
    public static class VariableReplacer
    {
        public static string ReplaceVariables(string template, PipelineExecutionContext context, Dictionary<string, object> input)
        {
            // Replace {{property}} style variables
            var result = Regex.Replace(
                template,
                @"\{\{([\w\.\[\]\*]+)\}\}",
                match =>
                {
                    var path = match.Groups[1].Value;
                    return GetDictionaryValue(input, path);
                }
            );

            // Replace ${context.path.to.value} style variables
            result = Regex.Replace(
                result,
                @"\${context\.([\w\.\[\]\*]+)}",
                match => context.GetValueFromPath<string>(match.Groups[1].Value, string.Empty)
            );

            // Replace ${input.path.to.value} style variables
            result = Regex.Replace(
                result,
                @"\${input\.([\w\.\[\]\*]+)}",
                match => GetDictionaryValue(input, match.Groups[1].Value)
            );

            // Replace ${env.VARIABLE_NAME} style variables
            result = Regex.Replace(
                result,
                @"\${env\.([\w\.]+)}",
                match => Environment.GetEnvironmentVariable(match.Groups[1].Value) ?? string.Empty
            );

            return result;
        }

        public static bool IsArrayPath(Dictionary<string, object> dict, string path)
        {
            try
            {
                var value = GetDictionaryValue<object>(dict, path);
                return value is List<object> || value is object[];
            }
            catch
            {
                return false;
            }
        }

        public static List<object> GetArrayValue(Dictionary<string, object> dict, string path)
        {
            try
            {
                var parts = path.Split('.');
                var arrayPath = parts.TakeWhile(p => !p.Contains("[*]")).ToList();
                var propertyPath = parts.SkipWhile(p => !p.Contains("[*]")).Skip(1).ToList();

                // Navigate to the array
                object current = dict;
                foreach (var part in arrayPath)
                {
                    if (current is not Dictionary<string, object> currentDict || !currentDict.TryGetValue(part, out current))
                    {
                        return new List<object>();
                    }
                }

                // Get the array field name (removing [*])
                var arrayField = parts[arrayPath.Count].Replace("[*]", "");
                if (current is not Dictionary<string, object> arrayDict || !arrayDict.TryGetValue(arrayField, out var arrayValue))
                {
                    return new List<object>();
                }

                // If we have a property path, extract that property from each array item
                if (propertyPath.Any())
                {
                    var remainingPath = string.Join(".", propertyPath);
                    if (arrayValue is List<Dictionary<string, object>> objectList)
                    {
                        return objectList.Select(item => GetDictionaryValue<object>(item, remainingPath)).ToList();
                    }
                    if (arrayValue is List<object> objList)
                    {
                        return objList.OfType<Dictionary<string, object>>()
                            .Select(item => GetDictionaryValue<object>(item, remainingPath))
                            .ToList();
                    }
                }

                // No property path, return the array items directly
                if (arrayValue is List<object> list)
                {
                    return list;
                }
                if (arrayValue is object[] array)
                {
                    return array.ToList();
                }
                if (arrayValue is List<Dictionary<string, object>> dictionaryList)
                {
                    return dictionaryList.Cast<object>().ToList();
                }

                return new List<object>();
            }
            catch
            {
                return new List<object>();
            }
        }

        private static string GetDictionaryValue(Dictionary<string, object> dict, string path)
        {
            var value = GetDictionaryValue<object>(dict, path);
            if (value is IEnumerable<object> enumerable && path.Contains("[*]"))
            {
                return JsonSerializer.Serialize(enumerable);
            }
            return value?.ToString() ?? string.Empty;
        }

        private static T GetDictionaryValue<T>(Dictionary<string, object> dict, string path)
        {
            try
            {
                var parts = path.Split('.');
                object current = dict;

                foreach (var part in parts)
                {
                    if (part.Contains("[*]"))
                    {
                        var arrayPart = part.Replace("[*]", "");
                        if (current is Dictionary<string, object> arrayDict && arrayDict.TryGetValue(arrayPart, out var arrayValue))
                        {
                            if (arrayValue is List<object> list)
                            {
                                var remainingPath = string.Join(".", parts.Skip(Array.IndexOf(parts, part) + 1));
                                if (!string.IsNullOrEmpty(remainingPath))
                                {
                                    var results = list.Select(item => 
                                    {
                                        if (item is Dictionary<string, object> itemDict)
                                        {
                                            return GetDictionaryValue<object>(itemDict, remainingPath);
                                        }
                                        return item;
                                    }).ToList();
                                    return (T)Convert.ChangeType(results, typeof(T));
                                }
                                return (T)Convert.ChangeType(list, typeof(T));
                            }
                            if (arrayValue is object[] array)
                            {
                                return (T)Convert.ChangeType(array.ToList(), typeof(T));
                            }
                        }
                        return default(T);
                    }

                    if (current is Dictionary<string, object> currentDict)
                    {
                        if (!currentDict.TryGetValue(part, out current))
                        {
                            return default(T);
                        }
                    }
                    else
                    {
                        return default(T);
                    }
                }

                if (current is T typedResult)
                {
                    return typedResult;
                }
                return (T)Convert.ChangeType(current, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
} 