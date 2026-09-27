using CodingChallenge.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodingChallenge.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private static AuthService CreateService()
    {
        var values = new Dictionary<string, string?>
        {
            ["Auth:Users:0:Username"] = "admin",
            ["Auth:Users:0:Password"] = "admin123",
            ["Auth:Users:0:Role"] = "Admin",
            ["Auth:Users:1:Username"] = "operator",
            ["Auth:Users:1:Password"] = "line42",
            ["Auth:Users:1:Role"] = "Operator"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new AuthService(configuration, Mock.Of<ILogger<AuthService>>());
    }

    [Test]
    public void Login_ReturnsConfiguredRole_AndResolvableToken()
    {
        var sut = CreateService();

        var result = sut.Login("operator", "line42");

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Username, Is.EqualTo("operator"));
            Assert.That(result.Role, Is.EqualTo("Operator"));
            Assert.That(sut.ResolveUser(result.Token), Is.EqualTo("operator"));
        });
    }

    [Test]
    public void Login_ReturnsNull_WhenPasswordIsWrong()
    {
        var sut = CreateService();

        var result = sut.Login("admin", "wrong-password");

        Assert.That(result, Is.Null);
    }
}
