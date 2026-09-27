using CodingChallenge.Api.Data;
using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Tests.Managers;

[TestFixture]
public class BoardManagerTests
{
    private AppDbContext _context = null!;
    private BoardManager _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new BoardManager(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_PersistsBoard()
    {
        var board = new Board { Name = "Board 1", Length = 100, Width = 50 };

        var created = await _sut.AddAsync(board);

        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(await _context.Boards.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_UpdatesFields_WhenBoardExists()
    {
        var board = new Board { Name = "Board 1", Length = 100, Width = 50 };
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var result = await _sut.UpdateAsync(new Board { Id = board.Id, Name = "Board 1 Updated", Length = 200, Width = 75 });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Board 1 Updated"));
        Assert.That(result.Length, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateAsync_ReturnsNull_WhenBoardDoesNotExist()
    {
        var result = await _sut.UpdateAsync(new Board { Id = 999, Name = "X", Length = 1, Width = 1 });

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SetComponentsAsync_ReplacesExistingLinks()
    {
        var component1 = new Component { Name = "C1", Quantity = 1 };
        var component2 = new Component { Name = "C2", Quantity = 1 };
        var board = new Board { Name = "Board 1", Length = 1, Width = 1 };
        _context.Components.AddRange(component1, component2);
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        await _sut.SetComponentsAsync(board.Id, new List<int> { component1.Id, component2.Id });
        var afterFirst = await _context.BoardComponents.Where(bc => bc.BoardId == board.Id).ToListAsync();
        Assert.That(afterFirst, Has.Count.EqualTo(2));

        await _sut.SetComponentsAsync(board.Id, new List<int> { component1.Id });
        var afterSecond = await _context.BoardComponents.Where(bc => bc.BoardId == board.Id).ToListAsync();

        Assert.That(afterSecond, Has.Count.EqualTo(1));
        Assert.That(afterSecond[0].ComponentId, Is.EqualTo(component1.Id));
    }

    [Test]
    public async Task DeleteAsync_RemovesBoard_WhenExists()
    {
        var board = new Board { Name = "Board 1", Length = 1, Width = 1 };
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(board.Id);

        Assert.That(result, Is.True);
        Assert.That(await _context.Boards.CountAsync(), Is.EqualTo(0));
    }
}
