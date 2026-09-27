namespace CodingChallenge.Api.Domain.Entities;

/// <summary>Join entity for the Order &lt;-&gt; Board many-to-many relationship.</summary>
public class OrderBoard
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
}
