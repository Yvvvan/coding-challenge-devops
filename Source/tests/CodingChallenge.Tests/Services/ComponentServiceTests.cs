using CodingChallenge.Api.Domain.Entities;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodingChallenge.Tests.Services;

[TestFixture]
public class ComponentServiceTests
{
    private Mock<IComponentManager> _componentManager = null!;
    private ComponentService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _componentManager = new Mock<IComponentManager>();
        _sut = new ComponentService(_componentManager.Object, Mock.Of<ILogger<ComponentService>>());
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenComponentDoesNotExist()
    {
        _componentManager.Setup(m => m.GetByIdAsync(1)).ReturnsAsync((Component?)null);

        var result = await _sut.GetByIdAsync(1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_MapsLinkedBoards()
    {
        var board = new Board { Id = 4, Name = "Board 4", Length = 1, Width = 1 };
        var component = new Component
        {
            Id = 2,
            Name = "Capacitor",
            Quantity = 5,
            BoardComponents = new List<BoardComponent> { new BoardComponent { BoardId = 4, ComponentId = 2, Board = board } }
        };

        _componentManager.Setup(m => m.GetByIdAsync(2)).ReturnsAsync(component);

        var result = await _sut.GetByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Boards, Has.Count.EqualTo(1));
        Assert.That(result.Boards[0].Name, Is.EqualTo("Board 4"));
    }

    [Test]
    public async Task CreateAsync_ReturnsCreatedComponent()
    {
        var dto = new ComponentCreateDto("Resistor", "1k ohm", 100);
        var created = new Component { Id = 3, Name = dto.Name, Description = dto.Description, Quantity = dto.Quantity };

        _componentManager.Setup(m => m.AddAsync(It.IsAny<Component>())).ReturnsAsync(created);

        var result = await _sut.CreateAsync(dto);

        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.Quantity, Is.EqualTo(100));
    }

    [Test]
    public void CreateAsync_RejectsNegativeQuantity()
    {
        var dto = new ComponentCreateDto("Invalid Component", null, -1);
        var created = new Component { Id = 10, Name = dto.Name, Quantity = dto.Quantity };

        _componentManager.Setup(m => m.AddAsync(It.IsAny<Component>())).ReturnsAsync(created);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.CreateAsync(dto));
        _componentManager.Verify(m => m.AddAsync(It.IsAny<Component>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_ReturnsNull_WhenComponentDoesNotExist()
    {
        _componentManager.Setup(m => m.UpdateAsync(It.IsAny<Component>())).ReturnsAsync((Component?)null);

        var result = await _sut.UpdateAsync(1, new ComponentUpdateDto("n", null, 1));

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ReturnsTrue_WhenComponentDeleted()
    {
        _componentManager.Setup(m => m.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        Assert.That(result, Is.True);
    }
}
