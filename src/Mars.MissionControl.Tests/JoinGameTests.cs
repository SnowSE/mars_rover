using Mars.MissionControl.Exceptions;
using System;
using System.Linq;

namespace Mars.MissionControl.Tests;

internal class JoinGameTests
{
    [Test]
    public void GameHasTarget()
    {
        var game = new Game();
        game.TargetLocation.Row.Should().BeGreaterThan(0);
        game.TargetLocation.Column.Should().BeGreaterThan(0);
    }

    [Test]
    public void CanJoinGame()
    {
        var game = new Game();
        var joinResult = game.Join("Jonathan");
        joinResult.Should().NotBeNull();
        joinResult.BatteryLevel.Should().Be(500);
    }

    [Test]
    public void GameGeneratesATokenValueObject()
    {
        var game = new Game();
        var joinResult = game.Join("P1");
        joinResult.Token.Should().BeOfType<PlayerToken>();
    }

    [Test]
    public void SecondPlayerCanJoin()
    {
        var game = new Game();
        var token1 = game.Join("P1");
        var token2 = game.Join("P2");
        token1.Should().NotBe(token2);
    }

    [Test]
    public void CannotJoinGameIfPlayerStateIsNotJoining()
    {
        var game = new Game();
        var token1 = game.Join("P1");
        game.PlayGame();
        Assert.Throws<InvalidGameStateException>(() => game.Join("P2"));
    }

    [Test]
    public void CannotPlayIfYouHaveNotJoined()
    {
        var game = new Game();
        game.PlayGame();
        var token1 = PlayerToken.Generate();
        Assert.Throws<UnrecognizedTokenException>(() => game.MovePerseverance(token1, Direction.Forward));
    }

    [Test]
    public void PlayerCannotMoveIfGameStateIsNotPlaying()
    {
        var game = new Game();
        var joinResult = game.Join("P1");
        var token1 = joinResult.Token;
        Assert.Throws<InvalidGameStateException>(() => game.MovePerseverance(token1, Direction.Forward));
    }

    [Test]
    public void WhenAPlayerJoinsTheyArePlacedOnTheMap()
    {
        var game = new Game();
        var joinResult = game.Join("P1");
        var token = joinResult.Token;
        var location = game.GetPlayerLocation(token);
        location.Should().NotBeNull();
    }

    [Test]
    public void TooManyPlayersCannotJoinGame()
    {
        var game = new Game(5, 5);
        foreach (var i in Enumerable.Range(0, 16))
        {
            game.Join($"P{i}");
        }
        Assert.Throws<TooManyPlayersException>(() => game.Join("P17"));
    }

    [TestCase(2, 2)]
    [TestCase(1, 2)]
    [TestCase(2, 1)]
    [TestCase(1, 1)]
    [TestCase(4, 4)]
    [TestCase(3, 3)]
    public void BoardMustBeAtLeast5x5(int rows, int cols)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Game(rows, cols));
    }
}
