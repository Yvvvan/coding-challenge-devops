using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodingChallenge.Tests.Services;

[TestFixture]
public class BoardServiceTests
{
    private Mock<IBoardManager> _boardManager = null!;
    private Mock<IComponentManager> _componentManager = null!;
    private BoardService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _boardManager = new Mock<IBoardManager>();
        _componentManager = new Mock<IComponentManager>();
        _sut = new BoardService(_boardManager.Object, _componentManager.Object, Mock.Of<ILogger<BoardService>>());
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenBoardDoesNotExist()
    {
        _boardManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Board?)null);

        var result = await _sut.GetByIdAsync(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsBoardWithComponentsAndOrders()
    {
        var order = new Order { Id = 1, Name = "Order 1", Status = OrderStatus.Active };
        var board = new Board
        {
            Id = 2,
            Name = "Board 2",
            Length = 100,
            Width = 50,
            OrderBoards = new List<OrderBoard> { new OrderBoard { OrderId = 1, BoardId = 2, Order = order } }
        };
        var components = new List<Component> { new Component { Id = 3, Name = "Resistor", Quantity = 10 } };

        _boardManager.Setup(m => m.GetByIdAsync(2)).ReturnsAsync(board);
        _componentManager.Setup(m => m.GetByBoardIdAsync(2)).ReturnsAsync(components);

        var result = await _sut.GetByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Orders, Has.Count.EqualTo(1));
        Assert.That(result.Components, Has.Count.EqualTo(1));
        Assert.That(result.Components[0].Name, Is.EqualTo("Resistor"));
    }

    [Test]
    public async Task CreateAsync_AddsBoardAndLinksComponents()
    {
        var dto = new BoardCreateDto("Board X", "desc", 10, 20, new List<int> { 1, 2 });
        var created = new Board { Id = 9, Name = dto.Name, Description = dto.Description, Length = dto.Length, Width = dto.Width };

        _boardManager.Setup(m => m.AddAsync(It.IsAny<Board>())).ReturnsAsync(created);
        _boardManager.Setup(m => m.GetByIdAsync(9)).ReturnsAsync(created);
        _componentManager.Setup(m => m.GetByBoardIdAsync(9)).ReturnsAsync(new List<Component>());

        var result = await _sut.CreateAsync(dto);

        Assert.That(result.Id, Is.EqualTo(9));
        _boardManager.Verify(m => m.SetComponentsAsync(9, dto.ComponentIds), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ReturnsNull_WhenBoardDoesNotExist()
    {
        _boardManager.Setup(m => m.UpdateAsync(It.IsAny<Board>())).ReturnsAsync((Board?)null);

        var result = await _sut.UpdateAsync(1, new BoardUpdateDto("n", null, 1, 1, new List<int>()));

        Assert.That(result, Is.Null);
        _boardManager.Verify(m => m.SetComponentsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_UpdatesBoardAndComponentLinks()
    {
        var dto = new BoardUpdateDto("Updated Board", "updated", 200, 100, new List<int> { 7, 8 });
        var updated = new Board
        {
            Id = 2,
            Name = dto.Name,
            Description = dto.Description,
            Length = dto.Length,
            Width = dto.Width
        };

        _boardManager.Setup(m => m.UpdateAsync(It.IsAny<Board>())).ReturnsAsync(updated);
        _boardManager.Setup(m => m.GetByIdAsync(2)).ReturnsAsync(updated);
        _componentManager.Setup(m => m.GetByBoardIdAsync(2)).ReturnsAsync(new List<Component>());

        var result = await _sut.UpdateAsync(2, dto);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Name, Is.EqualTo("Updated Board"));
            Assert.That(result.Length, Is.EqualTo(200));
            Assert.That(result.Width, Is.EqualTo(100));
        });
        _boardManager.Verify(m => m.SetComponentsAsync(2, dto.ComponentIds), Times.Once);
    }

    [Test]
    public void DeleteAsync_RejectsBoardReferencedByActiveOrder()
    {
        var order = new Order { Id = 1, Name = "Active Order", Status = OrderStatus.Active };
        var board = new Board
        {
            Id = 2,
            Name = "Board 2",
            OrderBoards = new List<OrderBoard>
            {
                new OrderBoard { OrderId = order.Id, BoardId = 2, Order = order }
            }
        };

        _boardManager.Setup(m => m.GetByIdAsync(board.Id)).ReturnsAsync(board);
        _boardManager.Setup(m => m.DeleteAsync(board.Id)).ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<BoardInUseException>(() => _sut.DeleteAsync(board.Id));

        Assert.That(ex!.BoardId, Is.EqualTo(board.Id));
        _boardManager.Verify(m => m.DeleteAsync(board.Id), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_AllowsBoardReferencedOnlyByDownloadedOrders()
    {
        var order = new Order { Id = 1, Name = "Downloaded Order", Status = OrderStatus.Downloaded };
        var board = new Board
        {
            Id = 2,
            Name = "Board 2",
            OrderBoards = new List<OrderBoard>
            {
                new OrderBoard { OrderId = order.Id, BoardId = 2, Order = order }
            }
        };

        _boardManager.Setup(m => m.GetByIdAsync(board.Id)).ReturnsAsync(board);
        _boardManager.Setup(m => m.DeleteAsync(board.Id)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(board.Id);

        Assert.That(result, Is.True);
        _boardManager.Verify(m => m.DeleteAsync(board.Id), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenBoardDoesNotExist()
    {
        _boardManager.Setup(m => m.GetByIdAsync(999)).ReturnsAsync((Board?)null);

        var result = await _sut.DeleteAsync(999);

        Assert.That(result, Is.False);
        _boardManager.Verify(m => m.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task SearchAsync_ReturnsMatchingBoards()
    {
        _boardManager.Setup(m => m.SearchAsync("Sensor")).ReturnsAsync(new List<Board>
        {
            new Board { Id = 1, Name = "Sensor Board", Length = 1, Width = 1 }
        });

        var result = await _sut.SearchAsync("Sensor");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Sensor Board"));
    }
}
