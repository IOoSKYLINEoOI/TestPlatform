namespace TestPlatform.Infrastructure.Files.ObjectStorage.Minio;

public class MinioStorageOptions
{
    public string Endpoint { get; set; } = "localhost:9000";

    public string PublicEndpoint { get; set; } = "http://localhost:9000";

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = "testplatform-images";

    public bool UseSsl { get; set; }

    public int PresignedUrlExpirySeconds { get; set; } = 3600;
}
