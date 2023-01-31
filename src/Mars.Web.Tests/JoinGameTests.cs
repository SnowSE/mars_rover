using Mars.MissionControl;
using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;
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
        var gameManager = _factory.Services.GetRequiredService<GameManager>();
        gameManager.StartNewGame(new GameStartOptions { Height = 5, Width = 5 });
        var client = _factory.CreateClient();
        var expectedLowResolutionMap = new[]
        {
            new LowResolutionMapTile
            {
                AverageDifficulty = 1,
                LowerLeftColumn= 0,
                LowerLeftRow= 0,
                UpperRightColumn = 4,
                UpperRightRow = 4
            }
        };

        var joinResponse = await client.GetFromJsonAsync<JoinResponse>("/game/join?name=Jonathan");

        joinResponse.Token.Length.Should().Be(13);
        joinResponse.LowResolutionMap.Should().BeEquivalentTo(expectedLowResolutionMap);
        joinResponse.Neighbors.Count().Should().BeOneOf(9, 12, 15);//9 if you're in a corner, 12 if you're one away from a corner, 15 if you're 2 away from a corner.
        joinResponse.TargetRow.Should().BeGreaterThan(0);
        joinResponse.TargetColumn.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task JoinGame_MissingName()
    {
        var client = _factory.CreateClient();

        var joinResponse = await client.GetAsync("/game/join");
        joinResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

}
