using CodingChallenge.Api.Data;
using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Tests.Managers;

[TestFixture]
public class ComponentManagerTests
{
    private AppDbContext _context = null!;
    private ComponentManager _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new ComponentManager(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetByBoardIdAsync_ReturnsOnlyComponentsLinkedToBoard()
    {
        var component1 = new Component { Name = "C1", Quantity = 1 };
        var component2 = new Component { Name = "C2", Quantity = 1 };
        var board1 = new Board { Name = "Board 1", Length = 1, Width = 1 };
        var board2 = new Board { Name = "Board 2", Length = 1, Width = 1 };
        _context.Components.AddRange(component1, component2);
        _context.Boards.AddRange(board1, board2);
        await _context.SaveChangesAsync();

        _context.BoardComponents.Add(new BoardComponent { BoardId = board1.Id, ComponentId = component1.Id });
        _context.BoardComponents.Add(new BoardComponent { BoardId = board2.Id, ComponentId = component2.Id });
        await _context.SaveChangesAsync();

        var result = await _sut.GetByBoardIdAsync(board1.Id);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(component1.Id));
    }

    [Test]
    public async Task GetByBoardIdAsync_DoesNotTrackReadOnlyComponentCatalog()
    {
        var linked = new Component { Name = "Linked", Quantity = 1 };
        var unrelated = new Component { Name = "Unrelated", Quantity = 1 };
        var board = new Board { Name = "Board 1", Length = 1, Width = 1 };
        _context.Components.AddRange(linked, unrelated);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        _context.BoardComponents.Add(new BoardComponent { BoardId = board.Id, ComponentId = linked.Id });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _sut.GetByBoardIdAsync(board.Id);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(_context.ChangeTracker.Entries(), Is.Empty);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenComponentDoesNotExist()
    {
        var result = await _sut.DeleteAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SearchAsync_ReturnsAll_WhenTermIsNullOrEmpty()
    {
        _context.Components.AddRange(
            new Component { Name = "C1", Quantity = 1 },
            new Component { Name = "C2", Quantity = 1 });
        await _context.SaveChangesAsync();

        var result = await _sut.SearchAsync(null);

        Assert.That(result, Has.Count.EqualTo(2));
    }
}
