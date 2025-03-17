using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserRepository
{
    private readonly DatabaseHelper _dbHelper;

    public UserRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task AddUserAsync(User user)
    {
        const string query = "INSERT INTO users (email, password_hash) VALUES (@Email, @PasswordHash)";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Email", user.Email);
        command.Parameters.AddWithValue("PasswordHash", user.PasswordHash);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        const string query = "SELECT id, email, created_at FROM users";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var users = new List<User>();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                CreatedAt = reader.GetDateTime(2)
            });
        }

        return users;
    }
}
