public interface IImageProcessingService
{
    Task<string?> ExtractAndProcessImageAsync(string url, string articleUrl);
    Task<string?> ResizeImageAsync(byte[] imageData, int targetWidth, int targetHeight);
    Task<bool> ValidateImageAsync(byte[] imageData);
    Task<string?> ConvertToBase64Async(byte[] imageData);
} 