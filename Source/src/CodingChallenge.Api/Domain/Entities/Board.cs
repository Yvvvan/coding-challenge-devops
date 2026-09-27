namespace CodingChallenge.Api.Domain.Entities;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
    public ICollection<BoardComponent> BoardComponents { get; set; } = new List<BoardComponent>();
}
