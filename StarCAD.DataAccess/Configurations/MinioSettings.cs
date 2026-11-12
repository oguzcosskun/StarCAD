namespace StarCAD.DataAccess.Configurations;

public class MinioSettings
{
    public string Endpoint { get; set; } = default!;

    /// <summary>
    /// Tarayıcıya verilen presigned URL'in host'u. Boşsa <see cref="Endpoint"/> kullanılır.
    /// API konteynerde koşarken MinIO'ya "minio:9000" ile ulaşır, ama o adı tarayıcı
    /// çözemez; SigV4 host'u da imzaladığı için URL sonradan yeniden yazılamaz.
    /// </summary>
    public string? PublicEndpoint { get; set; }

    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public bool WithSsL { get; set; }
    public int PresignExpirySeconds { get; set; } = 3600;
}