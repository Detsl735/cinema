using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WatchHistoryRepository
{
    private readonly DatabaseHelper _db;

    public WatchHistoryRepository(DatabaseHelper db)
    {
        _db = db;
    }

    public async Task AddAsync(int userId, int movieId)
    {
        const string sql = @"
            INSERT INTO watch_history (user_id, movie_id)
            VALUES (@user_id, @movie_id)
            ON CONFLICT DO NOTHING;";

        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("movie_id", movieId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<WatchHistoryItemDto>> GetWithTitlesAsync(int userId)
    {
        const string sql = @"
            SELECT w.id, w.movie_id, m.title, w.watched_at
            FROM watch_history w
            JOIN movies m ON m.id = w.movie_id
            WHERE w.user_id = @user_id
            ORDER BY w.watched_at DESC";

        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync();

        var result = new List<WatchHistoryItemDto>();
        while (await reader.ReadAsync())
        {
            result.Add(new WatchHistoryItemDto
            {
                Id = reader.GetInt32(0),
                MovieId = reader.GetInt32(1),
                MovieTitle = reader.GetString(2),
                WatchedAt = reader.GetDateTime(3)
            });
        }

        return result;
    }
}
