using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly FileService _fileService;
    private readonly MinioService _minioService;

    public FileController(FileService fileService, MinioService minioService)
    {
        _fileService = fileService;
        _minioService = minioService;
    }




    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(2L * 1024 * 1024 * 1024)]         
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile(
        [FromForm] int movieId,
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран или он пустой.");

        // весь workflow перенесён в сервис
        await _fileService.UploadFileAsync(movieId, file);

        return Ok("Файл успешно загружен и сконвертирован в HLS.");
    }


    [HttpGet]
    public async Task<IActionResult> GetAllFiles()
    {
        var files = await _fileService.GetAllFilesAsync();
        return Ok(files);
    }


    [HttpGet("{movieId}")]
    public async Task<IActionResult> GetFile(int movieId)
    {
        var file = await _fileService.GetFileByMovieIdAsync(movieId);
        var root = Path.GetFileNameWithoutExtension(file.FileUrl); // "IMG_7261"
        var signed = await _minioService.GeneratePresignedHlsAsync(root);
        // =====================================

        return Ok(new
        {
            file.Id,
            file.MovieId,
            hlsUrl = signed.ManifestUrl,   
            file.FileType,
            file.FileSize,
            file.UploadedAt
        });
    }
}
