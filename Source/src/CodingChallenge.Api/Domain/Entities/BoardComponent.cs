namespace CodingChallenge.Api.Domain.Entities;

/// <summary>Join entity for the Board &lt;-&gt; Component many-to-many relationship.</summary>
public class BoardComponent
{
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;
}
