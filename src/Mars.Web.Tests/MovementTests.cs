using Mars.MissionControl;
using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
namespace Mars.Web.Tests;

public class MovementTests
{
    private WebApplicationFactory<Program> _factory;
    private GameManager gameManager;
    private HttpClient client;
    private JoinResponse player1;

    [SetUp]
    public async Task Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {

                });
            });
        gameManager = _factory.Services.GetRequiredService<GameManager>();
        gameManager.StartNewGame(new GameStartOptions { Height = 5, Width = 5 });

        client = _factory.CreateClient();
        player1 = await client.GetFromJsonAsync<JoinResponse>("/game/join?name=P1");
        gameManager.PlayGame(new GamePlayOptions { MaxPlayerMessagesPerSecond = 1, RechargePointsPerSecond = 1 });
    }

    [Test]
    public async Task P1Moves()
    {
        var token = player1.Token;
        var direction = Direction.Forward;
        var response = await client.GetFromJsonAsync<MoveResponse>($"/game/moveperseverance?token={token}&direction={direction}");
        response.Should().NotBeNull();
    }

    [Test]
    public async Task P1CannotMoveWithInvalidDirection()
    {
        var token = player1.Token;
        var response = await client.GetAsync($"/game/moveperseverance?token={token}&direction=bogus");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Test]
    public async Task P1CannotMoveWithoutToken()
    {
        var token = player1.Token;
        var response = await client.GetAsync($"/game/moveperseverance");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
