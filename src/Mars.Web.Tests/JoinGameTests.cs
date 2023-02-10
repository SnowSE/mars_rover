using Mars.MissionControl;
using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;

namespace Mars.Web.Tests;

public class JoinGameTests
{
    private WebApplicationFactory<Program> _factory;
    private MultiGameHoster multiGameHoster;
    private string gameId;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        _factory = IntegrationTestHelper.CreateFactory();

        multiGameHoster = _factory.Services.GetRequiredService<MultiGameHoster>();
        gameId = multiGameHoster.MakeNewGame();
        gameManager = multiGameHoster.Games[gameId];
    }

    [Test]
    public async Task JoinGame()
    {
        var map = Helpers.CreateMap(5, 5);
        gameManager.StartNewGame(new GameStartOptions { Map = map });
        var client = _factory.CreateClient();
        var expectedLowResolutionMap = new[]
        {
            new LowResolutionMapTile
            {
                AverageDifficulty = 1,
                LowerLeftY= 0,
                LowerLeftX= 0,
                UpperRightY = 4,
                UpperRightX = 4
            }
        };

        var joinResponse = await client.GetFromJsonAsync<JoinResponse>($"/game/join?gameId={gameId}&name=Jonathan");

        joinResponse.Token.Length.Should().Be(13);
        joinResponse.LowResolutionMap.Should().BeEquivalentTo(expectedLowResolutionMap);
        joinResponse.Neighbors.Count().Should().BeOneOf(9, 12, 15);//9 if you're in a corner, 12 if you're one away from a corner, 15 if you're 2 away from a corner.
        joinResponse.TargetX.Should().BeGreaterThan(0);
        joinResponse.TargetY.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task JoinGame_MissingName()
    {
        var client = _factory.CreateClient();

        var joinResponse = await client.GetAsync("/game/join");
        joinResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }



    [Test]
    public async Task CannotMoveWithGameInJoiningState()
    {
        var client = _factory.CreateClient();

        var joinResponse = await client.GetFromJsonAsync<JoinResponse>($"/game/join?gameId={gameId}&name=p1");
        var moveResponse = await client.GetAsync($"/game/moveperseverance?token={joinResponse.Token}&move=Forward");
        var content = await moveResponse.Content.ReadAsStringAsync();
        content.Should().Contain("invalid game state");
    }

}
