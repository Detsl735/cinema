using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ReviewRepository
{
    private readonly DatabaseHelper _dbHelper;

    public ReviewRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task AddReviewAsync(Review review)
    {
        const string query = "INSERT INTO reviews (user_id, movie_id, rating, review_text) VALUES (@UserId, @MovieId, @Rating, @ReviewText)";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("UserId", review.UserId);
        command.Parameters.AddWithValue("MovieId", review.MovieId);
        command.Parameters.AddWithValue("Rating", review.Rating);
        command.Parameters.AddWithValue("ReviewText", review.ReviewText);

        await command.ExecuteNonQueryAsync();
    }
}
