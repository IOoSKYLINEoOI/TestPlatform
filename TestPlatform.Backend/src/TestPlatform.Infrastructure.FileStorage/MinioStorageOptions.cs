namespace TestPlatform.Infrastructure.FileStorage;

public class MinioStorageOptions
{
    public string Endpoint { get; set; } = "localhost:9000";

    public string PublicEndpoint { get; set; } = "http://localhost:9000";

    public string AccessKey { get; set; } = "minioadmin";

    public string SecretKey { get; set; } = "minioadmin";

    public string BucketName { get; set; } = "testplatform-images";

    public bool UseSsl { get; set; }

    public int PresignedUrlExpirySeconds { get; set; } = 3600;
}