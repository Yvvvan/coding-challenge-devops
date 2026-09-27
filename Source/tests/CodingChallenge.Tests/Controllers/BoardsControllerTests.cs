using CodingChallenge.Api.Controllers;
using CodingChallenge.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CodingChallenge.Tests.Controllers;

[TestFixture]
public class BoardsControllerTests
{
    [Test]
    public async Task Delete_ReturnsConflict_WhenBoardIsUsedByActiveOrder()
    {
        var service = new Mock<IBoardService>();
        service
            .Setup(s => s.DeleteAsync(2))
            .ThrowsAsync(new BoardInUseException(2));

        var sut = new BoardsController(service.Object);

        var result = await sut.Delete(2);

        var conflict = result as ConflictObjectResult;
        Assert.That(conflict, Is.Not.Null);
        Assert.That(conflict!.StatusCode, Is.EqualTo(409));
    }
}
