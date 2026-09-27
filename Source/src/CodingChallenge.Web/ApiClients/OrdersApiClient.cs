using System.Net.Http.Json;
using CodingChallenge.Web.Dtos;

namespace CodingChallenge.Web.ApiClients;

public class OrdersApiClient
{
    private readonly HttpClient _httpClient;

    public OrdersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<OrderDto>> GetAllAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/orders" : $"api/orders?search={Uri.EscapeDataString(search)}";
        return await _httpClient.GetFromJsonAsync<List<OrderDto>>(url) ?? new();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/orders/{id}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<OrderDto>() : null;
    }

    public async Task<OrderDto?> CreateAsync(OrderCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }

    public async Task<OrderDto?> UpdateAsync(int id, OrderUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/orders/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(byte[] Content, string FileName)?> DownloadAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/orders/{id}/download", null);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? $"order-{id}.json";
        return (bytes, fileName.Trim('"'));
    }
}
