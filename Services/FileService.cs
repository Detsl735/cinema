using System;
using System.IO;
using System.Threading.Tasks;

public class FileService
{
    private readonly MinioService _minioService;
    private readonly FFmpegService _ffmpegService;
    private readonly FileRepository _fileRepository;
    private readonly ILogger<FileService> _logger;

    public FileService(MinioService minioService, FFmpegService ffmpegService, FileRepository fileRepository, ILogger<FileService> logger)
    {
        _minioService = minioService;
        _ffmpegService = ffmpegService;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task UploadFileAsync(int movieId, string filePath, string bucketName)
    {
        var fileName = Path.GetFileName(filePath);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

        var fileUrl = $"https://localhost:9000/{bucketName}/{fileName}";
        var hlsUrl = $"https://localhost:9000/{bucketName}/{fileNameWithoutExt}.m3u8";
        var fileSize = new FileInfo(filePath).Length;

        _logger.LogInformation("Uploading file: FileUrl={FileUrl}, HlsUrl={HlsUrl}, FileType=mp4, FileSize={FileSize}",
            fileUrl, hlsUrl, fileSize);

        await _minioService.UploadFileAsync(bucketName, fileName, filePath, "video/mp4");

        var hlsDirectory = Path.GetDirectoryName(filePath);
        await _ffmpegService.ConvertToHlsAsync(filePath, hlsDirectory, fileNameWithoutExt);

        var hlsFilePath = $"{hlsDirectory}/{fileNameWithoutExt}.m3u8";
        await _minioService.UploadFileAsync(bucketName, $"{fileNameWithoutExt}.m3u8", hlsFilePath, "application/x-mpegURL");

        var file = new File
        {
            MovieId = movieId,
            FileUrl = fileUrl,
            HlsUrl = hlsUrl,
            FileType = "mp4",
            FileSize = fileSize,
            UploadedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Saving file to database: MovieId={MovieId}, FileUrl={FileUrl}, HlsUrl={HlsUrl}, FileType={FileType}, FileSize={FileSize}, UploadedAt={UploadedAt}",
            file.MovieId, file.FileUrl, file.HlsUrl, file.FileType, file.FileSize, file.UploadedAt);

        await _fileRepository.AddFileAsync(file);
    }

    public async Task<File> GetFileByMovieIdAsync(int movieId)
    {
        return await _fileRepository.GetFileByMovieIdAsync(movieId);
    }
}
