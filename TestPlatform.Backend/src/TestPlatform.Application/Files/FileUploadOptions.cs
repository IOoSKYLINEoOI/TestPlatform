namespace TestPlatform.Application.Files;

public class FileUploadOptions
{
    public int MaxFileSizeMb { get; set; } = 5;

    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".bmp",
    ];

    public int MaxWidth { get; set; } = 1024;

    public int MaxHeight { get; set; } = 1024;

    public int WebpQuality { get; set; } = 80;
}