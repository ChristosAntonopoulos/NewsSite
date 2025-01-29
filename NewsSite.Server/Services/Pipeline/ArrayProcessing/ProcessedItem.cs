using System;

namespace NewsSite.Server.Services.Pipeline.ArrayProcessing
{
    public class ProcessedItem
    {
        public object OriginalItem { get; set; }
        public object ProcessingResult { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
} 