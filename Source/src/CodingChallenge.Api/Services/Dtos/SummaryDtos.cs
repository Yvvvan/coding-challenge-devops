namespace CodingChallenge.Api.Services.Dtos;

public record BoardSummaryDto(int Id, string Name, double Length, double Width);

public record OrderSummaryDto(int Id, string Name, string Status);

public record ComponentSummaryDto(int Id, string Name, int Quantity);
