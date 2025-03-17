using System.Threading.Tasks;

public class ReviewService
{
    private readonly ReviewRepository _repository;

    public ReviewService(ReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task AddReviewAsync(Review review)
    {
        await _repository.AddReviewAsync(review);
    }
}
