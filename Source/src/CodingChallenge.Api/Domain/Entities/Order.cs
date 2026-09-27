namespace CodingChallenge.Api.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Active;

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
}
