using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchHistoryController : ControllerBase
{
    private readonly WatchHistoryService _service;
    public WatchHistoryController(WatchHistoryService svc) => _service = svc;

    // POST /api/watch-history
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] WatchHistoryDto dto)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _service.AddAsync(dto.MovieId, userId);
        return Ok();
    }

    // GET /api/watch-history
    [HttpGet]
    public async Task<IActionResult> List()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var list = await _service.GetWithTitlesAsync(userId);
        return Ok(list);
    }
}
