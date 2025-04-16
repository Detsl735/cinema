using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class FFmpegService
{
    /// <summary>
    /// Конвертирует входной MP4 в HLS: создаёт манифест и сегменты в папке 
    /// outputDirectory (в подпапке root), используя ffmpeg.
    /// </summary>
    /// <param name="inputPath">Путь к исходному .mp4 файлу</param>
    /// <param name="outputDirectory">Папка, куда складываем .m3u8 и папку с .ts</param>
    /// <param name="root">Базовое имя файла (без расширения), например "IMG_7261"</param>
    public async Task ConvertToHlsAsync(string inputPath, string outputDirectory, string root)
    {
        // 1) создаём папку для TS‑сегментов
        var segmentDir = Path.Combine(outputDirectory, root);
        Directory.CreateDirectory(segmentDir);

        // 2) формируем пути
        var manifestPath = Path.Combine(outputDirectory, $"{root}.m3u8");
        var segmentPattern = Path.Combine(segmentDir, "%d.ts");

        // 3) аргументы для ffmpeg
        var args =
            $"-i \"{inputPath}\" " +
            $"-codec copy " +
            $"-start_number 0 " +
            $"-hls_time 10 " +
            $"-hls_list_size 0 " +
            $"-f hls " +
            $"-hls_segment_filename \"{segmentPattern}\" " +
            $"\"{manifestPath}\"";

        var psi = new ProcessStartInfo
        {
            FileName = @"D:\Desktop\ffmpeg\bin\ffmpeg.exe", // укажите свой путь к ffmpeg.exe или просто "ffmpeg"
            Arguments = args,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Не удалось запустить ffmpeg");

        // читаем stderr на случай ошибки
        string stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
            throw new Exception($"FFmpeg завершился с кодом {proc.ExitCode}:\n{stderr}");
    }
}
