namespace NewsSite.Server.Models.Pipeline
{
    public interface IStepData
    {
        // Basic operations
        T GetValue<T>(string path, T defaultValue = default);
        void SetValue(string path, object value);
        bool HasValue(string path);
        void RemoveValue(string path);
        
        // Collection operations
        void AddToArray(string path, object value);
        void RemoveFromArray(string path, int index);
        int ArrayLength(string path);
        
        // Data transformation
        void Transform(string path, Func<object, object> transformer);
        void TransformArray(string path, Func<object, object> transformer);
        
        // String operations
        void ExtractKeywords(string sourcePath, string targetPath, int maxKeywords = 5);
        void SplitText(string sourcePath, string targetPath, params char[] separators);
        
        // Data filtering and aggregation
        void Filter(string path, Func<object, bool> predicate);
        void Aggregate<T>(string sourcePath, string targetPath, Func<IEnumerable<T>, object> aggregator);
        
        // Data merging
        void Merge(IStepData other, bool overwrite = true);
        void MergeAt(string path, IStepData other, bool overwrite = true);
        
        // Utility methods
        Dictionary<string, object> GetData();
        void Clear();
        IEnumerable<string> GetPaths();
    }
} 