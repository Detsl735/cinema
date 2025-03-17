using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly FileService _fileService;

    public FileController(FileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] int movieId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Файл не выбран или он пустой.");
        }

        var fileName = file.FileName;
        var tempDirectory = Path.Combine(Path.GetTempPath(), "uploads");

        if (!Directory.Exists(tempDirectory))
            Directory.CreateDirectory(tempDirectory);

        var tempFilePath = Path.Combine(tempDirectory, fileName);

        await using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        await _fileService.UploadFileAsync(movieId, tempFilePath, "movies-storage");

        return Ok("Файл успешно загружен и сконвертирован в HLS.");
    }



    [HttpGet("{movieId}")]
    public async Task<IActionResult> GetFile(int movieId)
    {
        var file = await _fileService.GetFileByMovieIdAsync(movieId);
        return Ok(file);
    }
}
