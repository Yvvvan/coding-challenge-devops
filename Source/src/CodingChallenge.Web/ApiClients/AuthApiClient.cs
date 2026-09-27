using System.Net.Http.Json;

namespace CodingChallenge.Web.ApiClients;

public record LoginRequestDto(string Username, string Password);

public record LoginResultDto(string Username, string Role, string Token);

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly UserSession _session;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, UserSession session, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _session = session;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Signing in user {Username} / {Password}", username, password);

        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDto(username, password));
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResultDto>();
        if (result is null)
        {
            return false;
        }

        _session.SignIn(result.Username, result.Role, result.Token);
        return true;
    }

    public void Logout() => _session.SignOut();
}
