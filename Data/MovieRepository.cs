using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MovieRepository
{
    private readonly DatabaseHelper _dbHelper;

    public MovieRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task AddMovieAsync(Movie movie)
    {
        const string query = "INSERT INTO movies (title, description, release_date, genre) VALUES (@title, @description, @release_date, @genre)";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("title", movie.Title);
        command.Parameters.AddWithValue("description", movie.Description);
        command.Parameters.AddWithValue("release_date", movie.ReleaseDate);
        command.Parameters.AddWithValue("genre", movie.Genre);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        const string query = "SELECT id, title, description, release_date, genre FROM movies";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var movies = new List<Movie>();
        while (await reader.ReadAsync())
        {
            movies.Add(new Movie
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                ReleaseDate = reader.GetDateTime(3),
                Genre = reader.GetString(4)
            });
        }

        return movies;
    }
}
