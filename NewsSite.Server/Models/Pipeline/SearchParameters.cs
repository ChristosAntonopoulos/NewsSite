using System.Collections.Generic;

namespace NewsSite.Server.Models.Pipeline
{
    public class SearchParameters
    {
        public int? Num { get; set; } = 10;
        public bool IncludeImages { get; set; } = false;
        public Dictionary<string, string> AdditionalParameters { get; set; }
        public string Engine { get; set; } = "google";
    }
} 