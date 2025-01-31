using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ImageProcessingSettings _settings;

    public ImageProcessingService(
        ILogger<ImageProcessingService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<ImageProcessingSettings> settings)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("ImageProcessing");
        _settings = settings.Value;
    }

    public async Task<string?> ExtractAndProcessImageAsync(string url, string articleUrl)
    {
        try
        {
            var imageUrl = await ExtractFeaturedImageAsync(url);
            if (imageUrl == null)
                return await GeneratePlaceholderImageAsync();

            var imageData = await DownloadImageAsync(imageUrl);
            if (imageData == null)
                return await GeneratePlaceholderImageAsync();

            if (!await ValidateImageAsync(imageData))
                return await GeneratePlaceholderImageAsync();

            var processedImage = await ResizeImageAsync(imageData, _settings.DefaultWidth, _settings.DefaultHeight);
            return processedImage ?? await GeneratePlaceholderImageAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image for {Url}", url);
            return await GeneratePlaceholderImageAsync();
        }
    }

    private async Task<string> ExtractFeaturedImageAsync(string url)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> ResizeImageAsync(byte[] imageData, int targetWidth, int targetHeight)
    {
        try
        {
            using var image = Image.Load(imageData);
            
            // Calculate dimensions maintaining aspect ratio
            var ratio = Math.Min((double)targetWidth / image.Width, (double)targetHeight / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x
                .Resize(newWidth, newHeight)
                .AutoOrient());

            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return null;
        }
    }

    public async Task<bool> ValidateImageAsync(byte[] imageData)
    {
        try
        {
            if (imageData.Length > _settings.MaxSizeKB * 1024) // Convert KB to bytes
                return false;

            using var image = Image.Load(imageData);
            return image.Width >= _settings.MinWidth && image.Height >= _settings.MinHeight; // Minimum dimensions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image");
            return false;
        }
    }

    public async Task<string?> ConvertToBase64Async(byte[] imageData)
    {
        try
        {
            return $"data:image/jpeg;base64,{Convert.ToBase64String(imageData)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to base64");
            return null;
        }
    }

    private async Task<byte[]?> DownloadImageAsync(string imageUrl)
    {
        for (int i = 0; i < _settings.MaxRetries; i++)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download image (attempt {Attempt})", i + 1);
                if (i < _settings.MaxRetries - 1)
                    await Task.Delay(1000 * (i + 1)); // Exponential backoff
            }
        }
        return null;
    }

    private async Task<string?> GeneratePlaceholderImageAsync()
    {
        // Generate a simple SVG placeholder
        var svg = @"<svg xmlns='http://www.w3.org/2000/svg' width='800' height='600' viewBox='0 0 800 600'>
            <rect width='100%' height='100%' fill='#f0f0f0'/>
            <text x='50%' y='50%' font-family='Arial' font-size='24' fill='#666' text-anchor='middle'>
                Image not available
            </text>
        </svg>";

        return $"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg))}";
    }
} 