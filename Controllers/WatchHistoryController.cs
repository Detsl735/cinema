using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class WatchHistoryController : ControllerBase
{
    private readonly WatchHistoryService _service;

    public WatchHistoryController(WatchHistoryService service)
    {
        _service = service;
    }

   
    [HttpPost]
    public async Task<IActionResult> AddHistory([FromBody] WatchHistory history)
    {
        await _service.AddHistoryAsync(history);
        return Ok("Watch history added successfully.");
    }

    
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<WatchHistory>>> GetHistoryByUser(int userId)
    {
        var history = await _service.GetHistoryByUserAsync(userId);
        return Ok(history);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] int progress)
    {
        await _service.UpdateProgressAsync(id, progress);
        return Ok("Watch progress updated successfully.");
    }
}
