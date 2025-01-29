namespace NewsSite.Server.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }

        public static ApiResponse<T> Ok(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Error(string message, string traceId = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                TraceId = traceId
            };
        }
    }
} 