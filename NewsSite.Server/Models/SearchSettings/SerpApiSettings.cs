namespace NewsSite.Server.Models.SearchSettings
{
    public class SerpApiSettings
    {
        public string SearchEngineId { get; set; }
        public int ResultsPerPage { get; set; } = 10;
        public int StartIndex { get; set; } = 1;
        public string SearchType { get; set; } = "";
        public string Language { get; set; } = "";
    }
} 