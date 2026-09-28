namespace CodingChallenge.Api.Services.Dtos;

public record ComponentDto(int Id, string Name, string? Description, int Quantity, List<BoardSummaryDto> Boards);

public record ComponentPageDto(List<ComponentDto> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public record ComponentCreateDto(string Name, string? Description, int Quantity);

public record ComponentUpdateDto(string Name, string? Description, int Quantity);
