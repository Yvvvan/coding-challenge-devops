namespace CodingChallenge.Web.ApiClients;

/// <summary>Attaches the access token of the current user to every outgoing API call.</summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly UserSession _session;

    public AuthTokenHandler(UserSession session)
    {
        _session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_session.IsAuthenticated)
        {
            request.Headers.Remove("X-Auth-Token");
            request.Headers.Add("X-Auth-Token", _session.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
