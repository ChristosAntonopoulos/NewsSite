namespace NewsSite.Server.Models
{
    public class ServiceResult<T>
    {
        public bool Success { get; }
        public T Data { get; }
        public string Error { get; }

        private ServiceResult(bool success, T data, string error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public static ServiceResult<T> Ok(T data) => new(true, data, null);
        public static ServiceResult<T> Fail(string error) => new(false, default, error);

        public ServiceResult<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return Success 
                ? ServiceResult<TNew>.Ok(mapper(Data))
                : ServiceResult<TNew>.Fail(Error);
        }
    }
} 