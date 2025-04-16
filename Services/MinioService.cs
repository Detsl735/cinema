using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Minio.DataModel.Args;
using System.Text;

public class MinioService
{
    private readonly IMinioClient _client;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioService> _logger;
    private readonly string _bucket = "movies-storage";

    public MinioService(IOptions<MinioSettings> settings, ILogger<MinioService> logger)
    {
        _settings = settings.Value;
        _logger = logger;


        _client = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .Build();
    }

    public async Task<PresignedHlsInfo> GeneratePresignedHlsAsync(string fileRoot)
    {
        // fileRoot = "IMG_7261"
        var manifestKey = $"{fileRoot}.m3u8";

        // Читаем оригинальный manifest
        using var ms = new MemoryStream();
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(manifestKey)
                .WithCallbackStream(s => s.CopyTo(ms)));
        ms.Position = 0;
        var lines = new StreamReader(ms).ReadToEnd().Split('\n');

        var sb = new StringBuilder();

        fileRoot = Path.GetFileNameWithoutExtension(manifestKey); // "IMG_7261"

        foreach (var line in lines)
        {
            var l = line.TrimEnd();

            if (string.IsNullOrWhiteSpace(l) || l.StartsWith("#"))
            {
                sb.AppendLine(l);
                continue;
            }
            if (l.StartsWith("http"))
            {
                sb.AppendLine(l);
                continue;
            }

            var tsKey = $"{fileRoot}/{l}";          // ← ключ с папкой
            var tsUrl = await _client.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(tsKey)
                    .WithExpiry(60 * 60));

            sb.AppendLine(tsUrl);
        }


        // Записываем «патч‑manifest» назад, рядом
        var patchedKey = $"{fileRoot}-signed.m3u8";
        var outBytes = Encoding.UTF8.GetBytes(sb.ToString());
        using var upload = new MemoryStream(outBytes);

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(patchedKey)
                .WithStreamData(upload)
                .WithObjectSize(upload.Length)
                .WithContentType("application/vnd.apple.mpegurl"));

        // Подписываем сам manifest
        var patchedUrl = await _client.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(patchedKey)
                .WithExpiry(60 * 60));

        return new PresignedHlsInfo { ManifestUrl = patchedUrl };
    }

    public Task UploadStreamAsync(string bucket, string objectName,
                              Stream data, string contentType, long size)
=> _client.PutObjectAsync(
       new PutObjectArgs()
           .WithBucket(bucket)
           .WithObject(objectName)
           .WithStreamData(data)
           .WithObjectSize(size)          // важно!
           .WithContentType(contentType));


    public record PresignedHlsInfo
    {
        public string ManifestUrl { get; init; } = default!;
    }


    public async Task UploadFileAsync(string bucketName, string objectName, string localPath, string contentType)
    {
        try
        {
            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(localPath)
                .WithContentType(contentType));

            _logger.LogInformation("Файл '{ObjectName}' успешно загружен в бакет '{BucketName}'.", objectName, bucketName);
        }
        catch (MinioException e)
        {
            _logger.LogError(e, "Ошибка при загрузке файла '{ObjectName}' в бакет '{BucketName}'.", objectName, bucketName);
        }
    }

    public async Task<bool> BucketExistsAsync(string bucketName)
    {
        try
        {
            bool exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            _logger.LogInformation("Проверка бакета '{BucketName}': {Exists}", bucketName, exists);
            return exists;
        }
        catch (MinioException e)
        {
            _logger.LogError(e, "Ошибка при проверке бакета '{BucketName}'.", bucketName);
            return false;
        }
    }

    public async Task CreateBucketAsync(string bucketName)
    {
        if (!await BucketExistsAsync(bucketName))
        {
            try
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                _logger.LogInformation("Бакет '{BucketName}' успешно создан.", bucketName);
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "Ошибка при создании бакета '{BucketName}'.", bucketName);
            }
        }
        else
        {
            _logger.LogInformation("Бакет '{BucketName}' уже существует.", bucketName);
        }
    }
}
