using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections;

namespace NewsSite.Server.Models.Pipeline
{
    public class StepData : IStepData
    {
        private readonly Dictionary<string, object> _data;

        public StepData()
        {
            _data = new Dictionary<string, object>();
        }

        public StepData(Dictionary<string, object> data)
        {
            _data = data ?? new Dictionary<string, object>();
        }

        // Basic operations
        public T GetValue<T>(string path, T defaultValue = default)
        {
            try
            {
                var value = GetValueInternal(path);
                if (value == null) return defaultValue;

                if (value is T typedValue) return typedValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetValue(string path, object value)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path cannot be empty");
            
            var parts = path.Split('.');
            var current = _data;
            
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]) || !(current[parts[i]] is Dictionary<string, object>))
                {
                    current[parts[i]] = new Dictionary<string, object>();
                }
                current = (Dictionary<string, object>)current[parts[i]];
            }
            
            current[parts[^1]] = value;
        }

        public bool HasValue(string path)
        {
            return GetValueInternal(path) != null;
        }

        public void RemoveValue(string path)
        {
            var parts = path.Split('.');
            if (parts.Length == 1)
            {
                _data.Remove(parts[0]);
                return;
            }

            var current = _data;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]) || !(current[parts[i]] is Dictionary<string, object>))
                    return;
                current = (Dictionary<string, object>)current[parts[i]];
            }

            current.Remove(parts[^1]);
        }

        // Collection operations
        public void AddToArray(string path, object value)
        {
            var array = GetValue<IList>(path);
            if (array == null)
            {
                array = new List<object>();
                SetValue(path, array);
            }
            array.Add(value);
        }

        public void RemoveFromArray(string path, int index)
        {
            var array = GetValue<IList>(path);
            if (array != null && index >= 0 && index < array.Count)
            {
                array.RemoveAt(index);
            }
        }

        public int ArrayLength(string path)
        {
            var array = GetValue<IList>(path);
            return array?.Count ?? 0;
        }

        // Data transformation
        public void Transform(string path, Func<object, object> transformer)
        {
            var value = GetValueInternal(path);
            if (value != null)
            {
                SetValue(path, transformer(value));
            }
        }

        public void TransformArray(string path, Func<object, object> transformer)
        {
            var array = GetValue<IList>(path);
            if (array != null)
            {
                var newArray = new List<object>();
                foreach (var item in array)
                {
                    newArray.Add(transformer(item));
                }
                SetValue(path, newArray);
            }
        }

        // String operations
        public void ExtractKeywords(string sourcePath, string targetPath, int maxKeywords = 5)
        {
            var text = GetValue<string>(sourcePath);
            if (string.IsNullOrEmpty(text)) return;

            var words = text.Split(new[] { ' ', ',', '.', '!', '?', '\n', '\r', '\t' }, 
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Select(w => w.ToLowerInvariant())
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(maxKeywords)
                .Select(g => g.Key)
                .ToList();

            SetValue(targetPath, words);
        }

        public void SplitText(string sourcePath, string targetPath, params char[] separators)
        {
            var text = GetValue<string>(sourcePath);
            if (string.IsNullOrEmpty(text)) return;

            var parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            SetValue(targetPath, parts);
        }

        // Data filtering
        public void Filter(string path, Func<object, bool> predicate)
        {
            var array = GetValue<IList>(path);
            if (array != null)
            {
                var filtered = array.Cast<object>().Where(predicate).ToList();
                SetValue(path, filtered);
            }
        }

        // Data aggregation
        public void Aggregate<T>(string sourcePath, string targetPath, Func<IEnumerable<T>, object> aggregator)
        {
            var array = GetValue<IList>(sourcePath);
            if (array != null)
            {
                var typed = array.Cast<T>();
                var result = aggregator(typed);
                SetValue(targetPath, result);
            }
        }

        // Data merging
        public void Merge(IStepData other, bool overwrite = true)
        {
            MergeDictionaries(_data, other.GetData(), overwrite);
        }

        public void MergeAt(string path, IStepData other, bool overwrite = true)
        {
            var target = GetOrCreateDictionary(path);
            MergeDictionaries(target, other.GetData(), overwrite);
        }

        // Utility methods
        public Dictionary<string, object> GetData() => _data;

        public void Clear() => _data.Clear();

        public IEnumerable<string> GetPaths()
        {
            return GetPathsInternal(_data, "");
        }

        // Private helper methods
        private object GetValueInternal(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var current = _data;
            var parts = path.Split('.');

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.TryGetValue(parts[i], out var value) || !(value is Dictionary<string, object> dict))
                    return null;
                current = dict;
            }

            current.TryGetValue(parts[^1], out var result);
            return result;
        }

        private Dictionary<string, object> GetOrCreateDictionary(string path)
        {
            var value = GetValueInternal(path);
            if (value is Dictionary<string, object> dict)
                return dict;

            dict = new Dictionary<string, object>();
            SetValue(path, dict);
            return dict;
        }

        private void MergeDictionaries(Dictionary<string, object> target, Dictionary<string, object> source, bool overwrite)
        {
            foreach (var kvp in source)
            {
                if (!target.ContainsKey(kvp.Key) || overwrite)
                {
                    if (kvp.Value is Dictionary<string, object> sourceDict)
                    {
                        if (!target.ContainsKey(kvp.Key))
                            target[kvp.Key] = new Dictionary<string, object>();

                        if (target[kvp.Key] is Dictionary<string, object> targetDict)
                            MergeDictionaries(targetDict, sourceDict, overwrite);
                        else if (overwrite)
                            target[kvp.Key] = sourceDict;
                    }
                    else
                    {
                        target[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        private IEnumerable<string> GetPathsInternal(Dictionary<string, object> dict, string prefix)
        {
            foreach (var kvp in dict)
            {
                var path = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                yield return path;

                if (kvp.Value is Dictionary<string, object> nested)
                {
                    foreach (var nestedPath in GetPathsInternal(nested, path))
                    {
                        yield return nestedPath;
                    }
                }
            }
        }
    }
} 