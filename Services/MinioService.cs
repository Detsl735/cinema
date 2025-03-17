using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Minio.DataModel.Args;

public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IOptions<MinioSettings> settings, ILogger<MinioService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .Build();
    }

    public async Task UploadFileAsync(string bucketName, string objectName, string filePath, string contentType)
    {
        try
        {
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
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
            bool exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
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
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
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
