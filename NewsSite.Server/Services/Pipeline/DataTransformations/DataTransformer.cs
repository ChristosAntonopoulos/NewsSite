using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace NewsSite.Server.Services.Pipeline.DataTransformations
{
    public class DataTransformer
    {
        public static object Transform(object input, string transformation)
        {
            if (input == null) return null;

            return transformation switch
            {
                "extract_keywords" => ExtractKeywords(input.ToString()),
                "to_list" => ConvertToList(input),
                "to_lowercase" => input.ToString().ToLower(),
                "to_uppercase" => input.ToString().ToUpper(),
                "trim" => input.ToString().Trim(),
                "remove_punctuation" => RemovePunctuation(input.ToString()),
                "extract_numbers" => ExtractNumbers(input.ToString()),
                "extract_urls" => ExtractUrls(input.ToString()),
                "extract_emails" => ExtractEmails(input.ToString()),
                "extract_hashtags" => ExtractHashtags(input.ToString()),
                _ => input
            };
        }

        private static List<string> ExtractKeywords(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            // Remove special characters and convert to lowercase
            text = Regex.Replace(text.ToLower(), @"[^\w\s]", " ");

            // Split into words
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Remove common stop words (you can expand this list)
            var stopWords = new HashSet<string> { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
            
            return words
                .Where(w => !stopWords.Contains(w) && w.Length > 2)
                .Distinct()
                .ToList();
        }

        private static List<object> ConvertToList(object input)
        {
            if (input is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    return jsonElement.EnumerateArray()
                        .Select(item => (object)JsonSerializer.Deserialize<object>(item.GetRawText()))
                        .ToList();
                }
            }
            
            if (input is IEnumerable<object> enumerable)
            {
                return enumerable.ToList();
            }

            // If it's a string, try to parse as JSON array
            if (input is string str)
            {
                try
                {
                    return JsonSerializer.Deserialize<List<object>>(str);
                }
                catch
                {
                    // If not valid JSON array, split by comma
                    return str.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Cast<object>()
                        .ToList();
                }
            }

            // If not a collection, return single item list
            return new List<object> { input };
        }

        private static string RemovePunctuation(string text)
        {
            return Regex.Replace(text, @"[^\w\s]", "");
        }

        private static List<string> ExtractNumbers(string text)
        {
            return Regex.Matches(text, @"\d+")
                .Select(m => m.Value)
                .ToList();
        }

        private static List<string> ExtractUrls(string text)
        {
            return Regex.Matches(text, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)")
                .Select(m => m.Value)
                .ToList();
        }

        private static List<string> ExtractEmails(string text)
        {
            return Regex.Matches(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")
                .Select(m => m.Value)
                .ToList();
        }

        private static List<string> ExtractHashtags(string text)
        {
            return Regex.Matches(text, @"#\w+")
                .Select(m => m.Value)
                .ToList();
        }
    }
} 