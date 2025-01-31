namespace NewsSite.Server.Exceptions
{
    public class NewsApiException : Exception
    {
        public int StatusCode { get; }

        public NewsApiException(string message, int statusCode = 500) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class PipelineNotFoundException : NewsApiException
    {
        public PipelineNotFoundException(int pipelineId) 
            : base($"Pipeline {pipelineId} not found", 404)
        {
        }
    }

    public class ArticleNotFoundException : NewsApiException
    {
        public ArticleNotFoundException(int articleId) 
            : base($"Article {articleId} not found", 404)
        {
        }
    }
} 