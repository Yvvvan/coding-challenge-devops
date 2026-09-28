using System.Net.Http.Json;
using CodingChallenge.Web.Dtos;

namespace CodingChallenge.Web.ApiClients;

public class ComponentsApiClient
{
    private readonly HttpClient _httpClient;

    public ComponentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ComponentDto>> GetAllAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/components" : $"api/components?search={Uri.EscapeDataString(search)}";
        return await _httpClient.GetFromJsonAsync<List<ComponentDto>>(url) ?? new();
    }

    public async Task<ComponentPageDto> GetPageAsync(string? search, int page, int pageSize)
    {
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        return await _httpClient.GetFromJsonAsync<ComponentPageDto>($"api/components/paged?{query}")
            ?? new ComponentPageDto(new List<ComponentDto>(), 0, page, pageSize, 0);
    }

    public async Task<ComponentDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/components/{id}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ComponentDto>() : null;
    }

    public async Task<ComponentDto?> CreateAsync(ComponentCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/components", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ComponentDto>();
    }

    public async Task<ComponentDto?> UpdateAsync(int id, ComponentUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/components/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ComponentDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/components/{id}");
        return response.IsSuccessStatusCode;
    }
}
