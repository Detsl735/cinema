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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
    {
        var movies = await _service.GetAllMoviesAsync();
        return Ok(movies);
    }
}
