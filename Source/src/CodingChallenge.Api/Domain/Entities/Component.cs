namespace CodingChallenge.Api.Domain.Entities;

public class Component
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }

    public ICollection<BoardComponent> BoardComponents { get; set; } = new List<BoardComponent>();
}
