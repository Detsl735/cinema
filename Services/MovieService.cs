using System.Collections.Generic;
using System.Threading.Tasks;

public class MovieService
{
    private readonly MovieRepository _repository;

    public MovieService(MovieRepository repository)
    {
        _repository = repository;
    }

    public async Task AddMovieAsync(Movie movie)
    {
        await _repository.AddMovieAsync(movie);
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        return await _repository.GetAllMoviesAsync();
    }
}
