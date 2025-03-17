using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WatchHistoryRepository
{
    private readonly DatabaseHelper _dbHelper;

    public WatchHistoryRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task AddHistoryAsync(WatchHistory history)
    {
        const string query = "INSERT INTO watch_history (user_id, movie_id, watched_at, progress) VALUES (@UserId, @MovieId, @WatchedAt, @Progress)";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("UserId", history.UserId);
        command.Parameters.AddWithValue("MovieId", history.MovieId);
        command.Parameters.AddWithValue("WatchedAt", history.WatchedAt);
        command.Parameters.AddWithValue("Progress", history.Progress);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<WatchHistory>> GetHistoryByUserAsync(int userId)
    {
        const string query = "SELECT * FROM watch_history WHERE user_id = @UserId";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("UserId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        var histories = new List<WatchHistory>();

        while (await reader.ReadAsync())
        {
            histories.Add(new WatchHistory
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                MovieId = reader.GetInt32(2),
                WatchedAt = reader.GetDateTime(3),
                Progress = reader.GetInt32(4)
            });
        }

        return histories;
    }

    public async Task UpdateProgressAsync(int id, int progress)
    {
        const string query = "UPDATE watch_history SET progress = @Progress WHERE id = @Id";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("Progress", progress);

        await command.ExecuteNonQueryAsync();
    }
}
