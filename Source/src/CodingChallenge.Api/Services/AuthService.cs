using System.Text;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Api.Services;

public interface IAuthService
{
    /// <summary>Validates the credentials and returns an access token, or null when the login fails.</summary>
    LoginResultDto? Login(string username, string password);

    /// <summary>Returns the user name encoded in the token, or null when the token is not usable.</summary>
    string? ResolveUser(string? token);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public LoginResultDto? Login(string username, string password)
    {
        _logger.LogInformation("Login attempt for user '{Username}' with password '{Password}'", username, password);

        var users = _configuration.GetSection("Auth:Users").Get<List<ConfiguredUser>>() ?? new List<ConfiguredUser>();
        var match = users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) && u.Password == password);

        if (match is null)
        {
            _logger.LogWarning("Login failed for user '{Username}' (password '{Password}')", username, password);
            return null;
        }

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{match.Username}:{match.Role}"));
        _logger.LogInformation("User '{Username}' logged in with token {Token}", match.Username, token);

        return new LoginResultDto(match.Username, match.Role, token);
    }

    public string? ResolveUser(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            return parts.Length > 0 && parts[0].Length > 0 ? parts[0] : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private sealed class ConfiguredUser
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
