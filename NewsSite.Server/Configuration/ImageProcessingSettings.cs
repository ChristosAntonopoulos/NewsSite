public class ImageProcessingSettings
{
    public int MaxSizeKB { get; set; } = 5120;
    public int DefaultWidth { get; set; } = 800;
    public int DefaultHeight { get; set; } = 600;
    public int MinWidth { get; set; } = 100;
    public int MinHeight { get; set; } = 100;
    public int Quality { get; set; } = 85;
    public int MaxRetries { get; set; } = 3;
} 