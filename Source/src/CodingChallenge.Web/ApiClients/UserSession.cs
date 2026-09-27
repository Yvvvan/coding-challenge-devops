namespace CodingChallenge.Web.ApiClients;

/// <summary>Holds the access token of the currently logged in user.</summary>
public class UserSession
{
    public string? Token { get; private set; }

    public string? Username { get; private set; }

    public string? Role { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    /// <summary>Raised whenever the sign in state changes so components can re-render.</summary>
    public event Action? Changed;

    public void SignIn(string username, string role, string token)
    {
        Username = username;
        Role = role;
        Token = token;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        Username = null;
        Role = null;
        Token = null;
        Changed?.Invoke();
    }
}
