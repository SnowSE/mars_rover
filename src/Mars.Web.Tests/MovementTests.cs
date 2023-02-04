using Mars.MissionControl;
using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
namespace Mars.Web.Tests;

public class MovementTests
{
    private WebApplicationFactory<Program> _factory;
    private MultiGameHoster multiGameHoster;
    private string gameId;
    private GameManager gameManager;
    private HttpClient client;
    private JoinResponse player1;
    private Orientation currentOrientation;
    private Location lastLocation, currentLocation;
    bool iWon;

    [SetUp]
    public async Task Setup()
    {
        _factory = IntegrationTestHelper.CreateFactory();

        multiGameHoster = _factory.Services.GetRequiredService<MultiGameHoster>();
        gameId = multiGameHoster.MakeNewGame();
        gameManager = multiGameHoster.Games[gameId];
        var map = Helpers.CreateMap(5, 5);
        gameManager.StartNewGame(new GameStartOptions { Map = map });

        client = _factory.CreateClient();
        player1 = await client.GetFromJsonAsync<JoinResponse>($"/game/join?gameId={gameId}&name=P1");
        currentOrientation = Enum.Parse<Orientation>(player1.Orientation);
        lastLocation = currentLocation = new Location(player1.StartingRow, player1.StartingColumn);
        iWon = false;

        gameManager.PlayGame(new GamePlayOptions { MaxPlayerMessagesPerSecond = 1, RechargePointsPerSecond = 1 });
    }

    [Test]
    public async Task P1GetsToTarget()
    {
        var token = player1.Token;
        if (player1.StartingRow != 0)//starting at top, move to left edge and move down
        {
            await turnToFace(Orientation.West);
            await driveForward();
            await turnToFace(Orientation.South);
            await driveForward();
            await turnToFace(Orientation.North);
        }
        else
        {
            await turnToFace(Orientation.West);
            await driveForward();
            await turnToFace(Orientation.North);
        }
        for (int i = 0; i < gameManager.Game.Board.Width; i++)
        {
            await driveForward();
            await turnToFace(Orientation.East);
            await driveForward(1);
            await turnToFace(Orientation.South);
            await driveForward();
            await turnToFace(Orientation.East);
            await driveForward(1);
            await turnToFace(Orientation.North);
        }

        if (iWon is false)
        {
            Assert.Fail("I drove across the entire board and never won. :( Gr.");
        }
    }

    private async Task driveForward(int spaces = int.MaxValue)
    {
        if (iWon)
        {
            return;
        }

        int spacesMoved = 0;
        do
        {
            lastLocation = currentLocation;
            var response = await client.GetFromJsonAsync<MoveResponse>($"/game/moveperseverance?token={player1.Token}&direction=Forward");
            currentLocation = new Location(response.Row, response.Column);
            spacesMoved++;

            if (response.Message == GameMessages.YouMadeItToTheTarget)
            {
                iWon = true;
                Assert.Pass("I won!!!");
                break;
            }
        } while (spacesMoved < spaces && currentLocation != lastLocation);
    }

    private async Task turnToFace(Orientation desiredOrientation)
    {
        while (currentOrientation != desiredOrientation)
        {
            var response = await client.GetFromJsonAsync<MoveResponse>($"/game/moveperseverance?token={player1.Token}&direction=Left");
            currentOrientation = Enum.Parse<Orientation>(response.Orientation);
        }
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
    public async Task MoveIngenuity()
    {
        var token = player1.Token;
        var destinationRow = player1.StartingRow -1_000;
        var destinationCol = player1.StartingColumn - 1_000;
        var response = await client.GetFromJsonAsync<IngenuityMoveResponse>(
            $"/game/moveingenuity?token={token}&destinationRow={destinationRow}&directionCol={destinationCol}");
        response.Message.Should().Be(GameMessages.IngenuityTooFar);
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
