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

    public async Task<IEnumerable<MovieCatalogItemDto>> GetCatalogAsync()
    {
        const string sql = "SELECT id, title FROM movies ORDER BY title";
        await using var conn = _dbHelper.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var result = new List<MovieCatalogItemDto>();
        while (await reader.ReadAsync())
        {
            result.Add(new MovieCatalogItemDto
            {
                MovieId = reader.GetInt32(0),
                Title = reader.GetString(1)
            });
        }

        return result;
    }

}
