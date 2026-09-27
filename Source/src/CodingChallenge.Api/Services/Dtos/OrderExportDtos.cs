namespace CodingChallenge.Api.Services.Dtos;

public record OrderExportDto(int Id, string Name, string? Description, DateTime OrderDate, List<BoardExportDto> Boards);

public record BoardExportDto(int Id, string Name, string? Description, double Length, double Width, List<ComponentSummaryDto> Components);
