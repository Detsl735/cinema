using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MovieController : ControllerBase
{
    private readonly MovieService _service;

    public MovieController(MovieService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AddMovie([FromBody] Movie movie)
    {
        await _service.AddMovieAsync(movie);
        return Ok("Movie added successfully!");
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var result = await _service.GetCatalogAsync();
        return Ok(result);
    }



}
