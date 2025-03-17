using System.Diagnostics;
using System.Threading.Tasks;

public class FFmpegService
{
    public async Task ConvertToHlsAsync(string inputPath, string outputDirectory, string outputFileName)
    {
        var outputPath = $"{outputDirectory}/{outputFileName}.m3u8";
        var ffmpegPath = @"D:\Desktop\ffmpeg-2025-03-13-git-958c46800e-essentials_build\bin\ffmpeg.exe"; 

        var arguments = $"-i {inputPath} -codec: copy -start_number 0 -hls_time 10 -hls_list_size 0 -f hls {outputPath}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }

}
