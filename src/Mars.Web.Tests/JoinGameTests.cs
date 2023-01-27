using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Mars.Web.Tests;

public class JoinGameTests
{
    private WebApplicationFactory<Program> _factory;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {

                });
            });
    }

    [Test]
    public async Task JoinGame()
    {
        var client = _factory.CreateClient();

        var joinResponse = await client.GetFromJsonAsync<JoinResponse>("/game/join?name=Jonathan");

        joinResponse.Token.Length.Should().Be(13);
    }

    [Test]
    public async Task JoinGame_MissingName()
    {
        var client = _factory.CreateClient();

        var joinResponse = await client.GetAsync("/game/join");
        joinResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

}
