namespace CodingChallenge.Web.ApiClients;

public class AdminApiClient
{
    private readonly HttpClient _httpClient;

    public AdminApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SeedLargeDatasetAsync()
    {
        var response = await _httpClient.PostAsync("api/admin/seed-large-dataset", null);
        return response.IsSuccessStatusCode;
    }
}
