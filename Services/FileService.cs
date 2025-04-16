using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public class FileService
{
    private readonly MinioService _minio;
    private readonly FFmpegService _ffmpeg;
    private readonly FileRepository _repo;
    private readonly ILogger<FileService> _log;

    // Имя бакета в MinIO
    private const string Bucket = "movies-storage";

    public FileService(
        MinioService minio,
        FFmpegService ffmpeg,
        FileRepository repo,
        ILogger<FileService> log)
    {
        _minio = minio;
        _ffmpeg = ffmpeg;
        _repo = repo;
        _log = log;
    }

    /// <summary>
    /// Основной метод, вызываемый из контроллера — принимает файл напрямую.
    /// </summary>
    public async Task UploadFileAsync(int movieId, IFormFile file)
    {
        // ─── Подготовка временной директории ──────────────────────
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempRoot);

        var fileName = file.FileName;
        var sourcePath = Path.Combine(tempRoot, fileName);

        // ─── Сохраняем IFormFile во временный путь ────────────────
        using (var stream = new FileStream(sourcePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // ─── Вызываем стандартный upload метод ────────────────────
        await UploadFileAsync(movieId, sourcePath);

        // ─── Удаляем временный файл ───────────────────────────────
        try
        {
            System.IO.File.Delete(sourcePath);
            ;
        }
        catch { }
    }

    /// <summary>
    /// Внутренний метод, работает с локальным mp4-файлом по пути
    /// </summary>
    public async Task UploadFileAsync(int movieId, string sourceMp4)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "hls-work");
        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, recursive: true);
        Directory.CreateDirectory(tempRoot);

        var baseName = Path.GetFileNameWithoutExtension(sourceMp4); // "IMG_7261"

        // ─── Конвертация в HLS ─────────────────────────────────────
        await _ffmpeg.ConvertToHlsAsync(sourceMp4, tempRoot, baseName);

        var manifestLocal = Path.Combine(tempRoot, $"{baseName}.m3u8");
        var segmentDir = Path.Combine(tempRoot, baseName);

        // ─── Загрузка manifest ─────────────────────────────────────
        await _minio.UploadFileAsync(
            Bucket,
            $"{baseName}.m3u8",
            manifestLocal,
            "application/vnd.apple.mpegurl");

        // ─── Загрузка всех ts-сегментов ────────────────────────────
        foreach (var tsPath in Directory.EnumerateFiles(segmentDir, "*.ts"))
        {
            var tsName = Path.GetFileName(tsPath);
            var objKey = $"{baseName}/{tsName}";

            await _minio.UploadFileAsync(
                Bucket,
                objKey,
                tsPath,
                "video/MP2T");
        }

        // ─── Запись в БД ───────────────────────────────────────────
        var fileRecord = new File
        {
            MovieId = movieId,
            FileUrl = $"http://localhost:9000/{Bucket}/{Path.GetFileName(sourceMp4)}",
            HlsUrl = $"http://localhost:9000/{Bucket}/{baseName}.m3u8",
            FileType = "mp4",
            FileSize = new FileInfo(sourceMp4).Length,
            UploadedAt = DateTime.UtcNow
        };

        await _repo.AddFileAsync(fileRecord);
        _log.LogInformation("HLS uploaded: MovieId={MovieId}, Manifest={HlsUrl}",
                            movieId, fileRecord.HlsUrl);

        // ─── Очистка временных HLS-файлов ─────────────────────────
        try { Directory.Delete(tempRoot, recursive: true); } catch { }
    }

    public Task<File> GetFileByMovieIdAsync(int id) =>
        _repo.GetFileByMovieIdAsync(id);

    public Task<IEnumerable<File>> GetAllFilesAsync() =>
        _repo.GetAllAsync();
}
