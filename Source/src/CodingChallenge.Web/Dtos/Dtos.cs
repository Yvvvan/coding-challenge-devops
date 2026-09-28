namespace CodingChallenge.Web.Dtos;

public record BoardSummaryDto(int Id, string Name, double Length, double Width);

public record OrderSummaryDto(int Id, string Name, string Status);

public record ComponentSummaryDto(int Id, string Name, int Quantity);

public record OrderDto(int Id, string Name, string? Description, DateTime OrderDate, string Status, List<BoardSummaryDto> Boards);

public record OrderCreateDto(string Name, string? Description, DateTime OrderDate, List<int> BoardIds);

public record OrderUpdateDto(string Name, string? Description, DateTime OrderDate, List<int> BoardIds);

public record BoardDto(int Id, string Name, string? Description, double Length, double Width, List<OrderSummaryDto> Orders, List<ComponentSummaryDto> Components);

public record BoardCreateDto(string Name, string? Description, double Length, double Width, List<int> ComponentIds);

public record BoardUpdateDto(string Name, string? Description, double Length, double Width, List<int> ComponentIds);

public record ComponentDto(int Id, string Name, string? Description, int Quantity, List<BoardSummaryDto> Boards);

public record ComponentPageDto(List<ComponentDto> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public record ComponentCreateDto(string Name, string? Description, int Quantity);

public record ComponentUpdateDto(string Name, string? Description, int Quantity);
