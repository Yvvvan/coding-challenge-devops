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
    public async Task SearchAsync_FiltersByNameOrDescription()
    {
        _context.Components.AddRange(
            new Component { Name = "Resistor", Description = "1k ohm", Quantity = 10 },
            new Component { Name = "Capacitor", Description = "ceramic sensor filter", Quantity = 5 },
            new Component { Name = "LED", Description = "status light", Quantity = 2 });
        await _context.SaveChangesAsync();

        var byName = await _sut.SearchAsync("Resistor");
        var byDescription = await _sut.SearchAsync("sensor");

        Assert.Multiple(() =>
        {
            Assert.That(byName.Select(c => c.Name), Is.EqualTo(new[] { "Resistor" }));
            Assert.That(byDescription.Select(c => c.Name), Is.EqualTo(new[] { "Capacitor" }));
        });
    }

    [Test]
    public async Task GetPageAsync_ReturnsRequestedPageAndTotalCount()
    {
        for (var i = 1; i <= 25; i++)
        {
            _context.Components.Add(new Component { Name = $"Component {i:D2}", Quantity = i });
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (items, totalCount) = await _sut.GetPageAsync(null, page: 2, pageSize: 10);

        Assert.Multiple(() =>
        {
            Assert.That(totalCount, Is.EqualTo(25));
            Assert.That(items, Has.Count.EqualTo(10));
            Assert.That(items[0].Name, Is.EqualTo("Component 11"));
            Assert.That(items[^1].Name, Is.EqualTo("Component 20"));
            Assert.That(_context.ChangeTracker.Entries(), Is.Empty);
        });
    }

    [Test]
    public async Task GetPageAsync_FiltersBeforePaging()
    {
        _context.Components.AddRange(
            new Component { Name = "Sensor A", Quantity = 1 },
            new Component { Name = "Sensor B", Quantity = 1 },
            new Component { Name = "Motor", Description = "Sensor drive", Quantity = 1 },
            new Component { Name = "LED", Quantity = 1 });
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _sut.GetPageAsync("Sensor", page: 1, pageSize: 2);

        Assert.Multiple(() =>
        {
            Assert.That(totalCount, Is.EqualTo(3));
            Assert.That(items, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task DeleteAsync_RemovesComponent_WhenExists()
    {
        var component = new Component { Name = "Temporary", Quantity = 1 };
        _context.Components.Add(component);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(component.Id);

        Assert.That(result, Is.True);
        Assert.That(await _context.Components.FindAsync(component.Id), Is.Null);
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
