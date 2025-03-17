using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ReviewService _service;

    public ReviewController(ReviewService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] Review review)
    {
        await _service.AddReviewAsync(review);
        return Ok("Review added successfully!");
    }
}
