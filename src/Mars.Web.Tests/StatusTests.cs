using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;

namespace Mars.Web.Tests;

public class StatusTests
{
    private WebApplicationFactory<Program> _factory;

    [SetUp]
    public void Setup()
    {
        _factory = IntegrationTestHelper.CreateFactory();
    }

    [Test]
    public async Task CanNotCallStatusWithOutToken()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/game/status");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CanNotCallStatusWithOutValidToken()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/game/status?token=ABC123");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
