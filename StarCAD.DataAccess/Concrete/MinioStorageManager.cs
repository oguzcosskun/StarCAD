using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using StarCAD.Core.Storage;              
using StarCAD.DataAccess.Configurations; 

namespace StarCAD.DataAccess.Concrete;
public class MinioStorageManager : IFileStorageService
{
    private readonly IMinioClient _client;
        /// <summary>
        /// Yalnızca presigned URL üretmek için: imza host'u tarayıcının ulaşabildiği
        /// adres olmalı. PublicEndpoint boşsa _client ile aynı istemcidir.
        /// </summary>
        private readonly IMinioClient _presignClient;
        private readonly MinioSettings _settings;

        public MinioStorageManager(IOptions<MinioSettings> options)
        {
            _settings = options.Value;
            _client = BuildClient(_settings, _settings.Endpoint);
            _presignClient = string.IsNullOrWhiteSpace(_settings.PublicEndpoint)
                ? _client
                : BuildClient(_settings, _settings.PublicEndpoint);
        }

        private static IMinioClient BuildClient(MinioSettings settings, string endpoint) =>
            new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.WithSsL)
                .Build();
    
        private async Task EnsureBucketAsync(CancellationToken ct)
        {
            bool found = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_settings.BucketName), ct);
            if (!found)
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName), ct);
        }
    
        public async Task<string> PutObjectAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default)
        {
            await EnsureBucketAsync(ct);
            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType), ct);
            return objectKey;
        }
    
        public async Task<Stream> GetObjectAsync(string objectKey, CancellationToken ct = default)
        {
            var ms = new MemoryStream();
            await _client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey)
                .WithCallbackStream(s => s.CopyTo(ms)), ct);
            ms.Position = 0;
            return ms;
        }
    
        public Task<string> CreatePresignedGetUrlAsync(string objectKey, CancellationToken ct = default) =>
            _presignClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey)
                .WithExpiry(_settings.PresignExpirySeconds));
    
        public async Task<bool> ObjectExistsAsync(string objectKey, CancellationToken ct = default)
        {
            try
            {
                await _client.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(_settings.BucketName).WithObject(objectKey), ct);
                return true;
            }
            catch { return false; }
        }
    
        public Task RemoveObjectAsync(string objectKey, CancellationToken ct = default) =>
            _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_settings.BucketName).WithObject(objectKey), ct);
}