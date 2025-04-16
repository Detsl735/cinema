using Npgsql;

public class UserRepository
{
    private readonly DatabaseHelper _dbHelper;

    public UserRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task AddUserAsync(User user)
    {
        const string query = "INSERT INTO users (email, password_hash, created_at, role) VALUES (@Email, @PasswordHash, @CreatedAt, @Role)";
        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Email", user.Email);
        command.Parameters.AddWithValue("PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("CreatedAt", user.CreatedAt);
        command.Parameters.AddWithValue("Role", user.Role);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        const string query = "SELECT * FROM users WHERE email = @Email";
        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Email", email);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                Role = reader.GetString(reader.GetOrdinal("role"))
            };
        }

        return null;
    }
}
