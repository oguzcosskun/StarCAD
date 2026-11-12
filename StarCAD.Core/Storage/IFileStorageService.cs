namespace StarCAD.Core.Storage;

public interface IFileStorageService
{
    Task<string> PutObjectAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> GetObjectAsync(string objectKey, CancellationToken ct = default);
    Task<string> CreatePresignedGetUrlAsync(string objectKey, CancellationToken ct = default);
    Task<bool>   ObjectExistsAsync(string objectKey, CancellationToken ct = default);
    Task         RemoveObjectAsync(string objectKey, CancellationToken ct = default);
}