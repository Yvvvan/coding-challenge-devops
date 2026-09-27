using CodingChallenge.Api.Domain.Entities;

namespace CodingChallenge.Api.Data.Seed;

/// <summary>Seeds a single Board with a very large number of Components, for reproducing performance issues at scale.</summary>
public static class LargeDatasetSeeder
{
    public const string StressTestBoardName = "Stress Test Board (50k Components)";
    private const int ComponentCount = 50_000;

    /// <returns>True if the large dataset was seeded, false if it already existed.</returns>
    public static bool SeedIfMissing(AppDbContext context)
    {
        if (context.Boards.Any(b => b.Name == StressTestBoardName))
        {
            return false;
        }

        var board = new Board
        {
            Name = StressTestBoardName,
            Description = "Auto-generated board used to reproduce performance issues with large component counts.",
            Length = 500,
            Width = 500
        };
        context.Boards.Add(board);

        // Bulk insert without change tracking overhead for 50k+ rows.
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            var components = new List<Component>(ComponentCount);
            for (var i = 1; i <= ComponentCount; i++)
            {
                components.Add(new Component
                {
                    Name = $"Stress Component {i}",
                    Description = "Auto-generated for performance testing",
                    Quantity = 1
                });
            }

            context.Components.AddRange(components);
            context.SaveChanges();

            var boardComponents = new List<BoardComponent>(ComponentCount);
            foreach (var component in components)
            {
                boardComponents.Add(new BoardComponent { BoardId = board.Id, ComponentId = component.Id });
            }

            context.BoardComponents.AddRange(boardComponents);
            context.SaveChanges();
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return true;
    }
}
