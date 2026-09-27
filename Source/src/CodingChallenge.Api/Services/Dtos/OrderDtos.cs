namespace CodingChallenge.Api.Services.Dtos;

public record OrderDto(int Id, string Name, string? Description, DateTime OrderDate, string Status, List<BoardSummaryDto> Boards);

public record OrderCreateDto(string Name, string? Description, DateTime OrderDate, List<int> BoardIds);

public record OrderUpdateDto(string Name, string? Description, DateTime OrderDate, List<int> BoardIds);
