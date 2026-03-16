namespace TestPlatform.Infrastructure.FileStorage;

public class ImageStorageOptions
{
    public string RootPath { get; set; } = "wwwroot/images";

    public string TempFolder { get; set; } = "temp";

    public string PermanentFolder { get; set; } = "permanent";

    public int MaxFileSizeMb { get; set; } = 5;

    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".bmp"
    ];

    public int MaxWidth { get; set; } = 1024;

    public int MaxHeight { get; set; } = 1024;

    public int WebpQuality { get; set; } = 80;
}
