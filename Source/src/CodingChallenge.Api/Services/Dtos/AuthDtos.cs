namespace CodingChallenge.Api.Services.Dtos;

public record LoginRequestDto(string Username, string Password);

public record LoginResultDto(string Username, string Role, string Token);
