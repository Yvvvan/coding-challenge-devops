using CodingChallenge.Api.Data;
using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Tests.Managers;

[TestFixture]
public class OrderManagerTests
{
    private AppDbContext _context = null!;
    private OrderManager _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new OrderManager(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_PersistsOrder()
    {
        var order = new Order { Name = "Order 1", OrderDate = DateTime.UtcNow, Status = OrderStatus.Active };

        var created = await _sut.AddAsync(order);

        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(await _context.Orders.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetByIdAsync(123);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SetBoardsAsync_ReplacesExistingLinks()
    {
        var board1 = new Board { Name = "Board 1", Length = 1, Width = 1 };
        var board2 = new Board { Name = "Board 2", Length = 1, Width = 1 };
        var order = new Order { Name = "Order 1", OrderDate = DateTime.UtcNow, Status = OrderStatus.Active };
        _context.Boards.AddRange(board1, board2);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _sut.SetBoardsAsync(order.Id, new List<int> { board1.Id });
        var afterFirst = await _context.OrderBoards.Where(ob => ob.OrderId == order.Id).ToListAsync();
        Assert.That(afterFirst, Has.Count.EqualTo(1));

        await _sut.SetBoardsAsync(order.Id, new List<int> { board2.Id });
        var afterSecond = await _context.OrderBoards.Where(ob => ob.OrderId == order.Id).ToListAsync();

        Assert.That(afterSecond, Has.Count.EqualTo(1));
        Assert.That(afterSecond[0].BoardId, Is.EqualTo(board2.Id));
    }

    [Test]
    public async Task DeleteAsync_RemovesOrder_WhenExists()
    {
        var order = new Order { Name = "Order 1", OrderDate = DateTime.UtcNow, Status = OrderStatus.Active };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(order.Id);

        Assert.That(result, Is.True);
        Assert.That(await _context.Orders.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var result = await _sut.DeleteAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SearchAsync_FiltersByNameOrDescription()
    {
        _context.Orders.AddRange(
            new Order { Name = "Pilot batch", OrderDate = DateTime.UtcNow, Status = OrderStatus.Active },
            new Order { Name = "Other", Description = "Pilot related", OrderDate = DateTime.UtcNow, Status = OrderStatus.Active },
            new Order { Name = "Unrelated", OrderDate = DateTime.UtcNow, Status = OrderStatus.Active });
        await _context.SaveChangesAsync();

        var result = await _sut.SearchAsync("Pilot");

        Assert.That(result, Has.Count.EqualTo(2));
    }
}
