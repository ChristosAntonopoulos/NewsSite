namespace NewsSite.Server.Services.Pipeline.ArrayProcessing
{
    public class ArrayNotation
    {
        public string OriginalPath { get; }
        public string BasePath { get; }
        public string PropertyPath { get; }
        public bool IsArrayNotation { get; }

        public ArrayNotation(string path)
        {
            OriginalPath = path;
            IsArrayNotation = path.Contains("[*]");
            
            if (IsArrayNotation)
            {
                // Remove {{ and }} and split the path
                var cleanPath = path.Trim('{', '}');
                var arrayIndex = cleanPath.IndexOf("[*]");
                BasePath = cleanPath.Substring(0, arrayIndex);
                PropertyPath = cleanPath.Substring(arrayIndex + 4); // +4 to skip "[*]."
            }
        }
    }
} 