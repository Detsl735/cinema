using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly UserRepository _userRepository;

    public AuthService(IConfiguration configuration, UserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public async Task RegisterAsync(string email, string password)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(email);
        if (existingUser != null)
            throw new Exception("User with this email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            Role = "User"
        };

        await _userRepository.AddUserAsync(user);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
