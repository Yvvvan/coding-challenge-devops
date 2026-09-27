using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodingChallenge.Tests.Services;

[TestFixture]
public class OrderServiceTests
{
    private Mock<IOrderManager> _orderManager = null!;
    private Mock<IBoardManager> _boardManager = null!;
    private Mock<IComponentManager> _componentManager = null!;
    private OrderService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _orderManager = new Mock<IOrderManager>();
        _boardManager = new Mock<IBoardManager>();
        _componentManager = new Mock<IComponentManager>();
        _sut = new OrderService(_orderManager.Object, _boardManager.Object, _componentManager.Object, Mock.Of<ILogger<OrderService>>());
    }

    [Test]
    public async Task CreateAsync_AddsOrderAndLinksBoards()
    {
        var dto = new OrderCreateDto("Order X", "desc", new DateTime(2026, 1, 1), new List<int> { 1, 2 });
        var created = new Order { Id = 5, Name = dto.Name, Description = dto.Description, OrderDate = dto.OrderDate, Status = OrderStatus.Active };

        _orderManager.Setup(m => m.AddAsync(It.IsAny<Order>())).ReturnsAsync(created);
        _orderManager.Setup(m => m.GetByIdAsync(5)).ReturnsAsync(created);

        var result = await _sut.CreateAsync(dto);

        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.Name, Is.EqualTo("Order X"));
        Assert.That(result.Status, Is.EqualTo(nameof(OrderStatus.Active)));
        _orderManager.Verify(m => m.SetBoardsAsync(5, dto.BoardIds), Times.Once);
    }

    [Test]
    public void CreateAsync_RejectsOrderWithoutBoards()
    {
        var dto = new OrderCreateDto("Order without boards", null, DateTime.UtcNow, new List<int>());
        var created = new Order
        {
            Id = 10,
            Name = dto.Name,
            OrderDate = dto.OrderDate,
            Status = OrderStatus.Active
        };

        _orderManager.Setup(m => m.AddAsync(It.IsAny<Order>())).ReturnsAsync(created);
        _orderManager.Setup(m => m.GetByIdAsync(created.Id)).ReturnsAsync(created);

        Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
        _orderManager.Verify(m => m.AddAsync(It.IsAny<Order>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        _orderManager.Setup(m => m.GetByIdAsync(99)).ReturnsAsync((Order?)null);

        var result = await _sut.UpdateAsync(99, new OrderUpdateDto("n", null, DateTime.UtcNow, new List<int>()));

        Assert.That(result, Is.Null);
        _orderManager.Verify(m => m.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_PreservesExistingStatus()
    {
        var existing = new Order { Id = 3, Name = "Old", OrderDate = DateTime.UtcNow, Status = OrderStatus.Downloaded };
        var updated = new Order { Id = 3, Name = "New", OrderDate = DateTime.UtcNow, Status = OrderStatus.Downloaded };

        _orderManager.SetupSequence(m => m.GetByIdAsync(3))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);
        _orderManager.Setup(m => m.UpdateAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);

        var dto = new OrderUpdateDto("New", null, DateTime.UtcNow, new List<int>());
        await _sut.UpdateAsync(3, dto);

        _orderManager.Verify(m => m.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Downloaded)), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenOrderDoesNotExist()
    {
        _orderManager.Setup(m => m.DeleteAsync(42)).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(42);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DownloadAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        _orderManager.Setup(m => m.GetByIdAsync(7)).ReturnsAsync((Order?)null);

        var result = await _sut.DownloadAsync(7);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DownloadAsync_MarksOrderAsDownloaded_AndReturnsJsonContent()
    {
        var board = new Board { Id = 1, Name = "B1", Length = 10, Width = 20 };
        var order = new Order
        {
            Id = 1,
            Name = "Order 1",
            OrderDate = new DateTime(2026, 1, 1),
            Status = OrderStatus.Active,
            OrderBoards = new List<OrderBoard> { new OrderBoard { OrderId = 1, BoardId = 1, Board = board } }
        };

        _orderManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(order);
        _componentManager.Setup(m => m.GetByBoardIdAsync(1)).ReturnsAsync(new List<Component>());
        _orderManager.Setup(m => m.UpdateAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);

        var result = await _sut.DownloadAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.Content.Length, Is.GreaterThan(0));
        Assert.That(result.Value.FileName, Does.StartWith("order-1-"));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Downloaded));
    }
}
