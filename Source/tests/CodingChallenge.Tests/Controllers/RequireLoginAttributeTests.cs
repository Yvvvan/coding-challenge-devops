using CodingChallenge.Api.Controllers;
using CodingChallenge.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CodingChallenge.Tests.Controllers;

[TestFixture]
public class RequireLoginAttributeTests
{
    [Test]
    public void OnActionExecuting_ReturnsUnauthorized_WhenTokenIsMissing()
    {
        var authService = new Mock<IAuthService>();
        authService.Setup(s => s.ResolveUser(null)).Returns((string?)null);

        var services = new ServiceCollection()
            .AddSingleton(authService.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        var filterContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: new object());

        var sut = new RequireLoginAttribute();

        sut.OnActionExecuting(filterContext);

        Assert.That(filterContext.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }
}
