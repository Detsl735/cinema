using Npgsql;
using System.Threading.Tasks;

public class FileRepository
{
    private readonly DatabaseHelper _dbHelper;

    public FileRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task AddFileAsync(File file)
    {
        const string query = "INSERT INTO files (movie_id, file_url, hls_url, file_type, file_size, uploaded_at) " +
                             "VALUES (@MovieId, @FileUrl, @HlsUrl, @FileType, @FileSize, @UploadedAt)";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("MovieId", file.MovieId);
        command.Parameters.AddWithValue("FileUrl", file.FileUrl);
        command.Parameters.AddWithValue("HlsUrl", file.HlsUrl);
        command.Parameters.AddWithValue("FileType", file.FileType);
        command.Parameters.AddWithValue("FileSize", file.FileSize);
        command.Parameters.AddWithValue("UploadedAt", file.UploadedAt);

        await command.ExecuteNonQueryAsync();
    }




    public async Task<File> GetFileByMovieIdAsync(int movieId)
    {
        const string query = "SELECT id, movie_id, file_url, hls_url, file_type, file_size, uploaded_at FROM files WHERE movie_id = @MovieId";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("MovieId", movieId);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new File
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                MovieId = reader.GetInt32(reader.GetOrdinal("movie_id")),
                FileUrl = reader.GetString(reader.GetOrdinal("file_url")),
                HlsUrl = reader.GetString(reader.GetOrdinal("hls_url")),
                FileType = reader.GetString(reader.GetOrdinal("file_type")),
                FileSize = reader.GetInt64(reader.GetOrdinal("file_size")),
                UploadedAt = reader.GetDateTime(reader.GetOrdinal("uploaded_at"))
            };
        }

        return null;
    }

    public async Task<IEnumerable<File>> GetAllAsync()
    {
        const string query = "SELECT * FROM files";

        await using var connection = _dbHelper.GetConnection();
        await connection.OpenAsync();

        var list = new List<File>();
        await using var cmd = new NpgsqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new File
            {
                Id = reader.GetInt32(0),
                MovieId = reader.GetInt32(1),
                FileUrl = reader.GetString(2),
                HlsUrl = reader.GetString(3),
                FileType = reader.GetString(4),
                FileSize = reader.GetInt64(5),
                UploadedAt = reader.GetDateTime(6)
            });
        }

        return list;
    }


}
