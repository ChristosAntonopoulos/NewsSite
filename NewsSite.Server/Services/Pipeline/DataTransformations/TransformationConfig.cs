namespace NewsSite.Server.Services.Pipeline.DataTransformations
{
    public class TransformationConfig
    {
        public string Field { get; set; }
        public string Transformation { get; set; }
        public bool ApplyToList { get; set; }
        public string OutputField { get; set; }
    }

    public static class TransformationTypes
    {
        public static readonly string[] AvailableTransformations = new[]
        {
            "extract_keywords",
            "to_list",
            "to_lowercase",
            "to_uppercase",
            "trim",
            "remove_punctuation",
            "extract_numbers",
            "extract_urls",
            "extract_emails",
            "extract_hashtags"
        };

        public static readonly Dictionary<string, string> TransformationDescriptions = new()
        {
            { "extract_keywords", "Extract meaningful keywords from text" },
            { "to_list", "Convert input to a list (splits by comma if string)" },
            { "to_lowercase", "Convert text to lowercase" },
            { "to_uppercase", "Convert text to uppercase" },
            { "trim", "Remove leading and trailing whitespace" },
            { "remove_punctuation", "Remove all punctuation marks" },
            { "extract_numbers", "Extract all numbers from text" },
            { "extract_urls", "Extract all URLs from text" },
            { "extract_emails", "Extract all email addresses from text" },
            { "extract_hashtags", "Extract all hashtags from text" }
        };
    }
} 