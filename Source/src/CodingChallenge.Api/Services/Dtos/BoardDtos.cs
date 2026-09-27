namespace CodingChallenge.Api.Services.Dtos;

public record BoardDto(int Id, string Name, string? Description, double Length, double Width, List<OrderSummaryDto> Orders, List<ComponentSummaryDto> Components);

public record BoardCreateDto(string Name, string? Description, double Length, double Width, List<int> ComponentIds);

public record BoardUpdateDto(string Name, string? Description, double Length, double Width, List<int> ComponentIds);
