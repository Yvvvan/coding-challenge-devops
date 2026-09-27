using System.Net.Http.Json;
using CodingChallenge.Web.Dtos;

namespace CodingChallenge.Web.ApiClients;

public class BoardsApiClient
{
    private readonly HttpClient _httpClient;

    public BoardsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BoardDto>> GetAllAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/boards" : $"api/boards?search={Uri.EscapeDataString(search)}";
        return await _httpClient.GetFromJsonAsync<List<BoardDto>>(url) ?? new();
    }

    public async Task<BoardDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/boards/{id}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BoardDto>() : null;
    }

    public async Task<BoardDto?> CreateAsync(BoardCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/boards", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BoardDto>();
    }

    public async Task<BoardDto?> UpdateAsync(int id, BoardUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/boards/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BoardDto>();
    }

    /// <returns>True if deleted; false if the API rejected the delete (e.g. not found or in use).</returns>
    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/boards/{id}");
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
    }
}
