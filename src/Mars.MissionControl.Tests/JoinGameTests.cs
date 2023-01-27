using Mars.MissionControl.Exceptions;
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
        var token = game.Join("Jonathan");
        token.Should().NotBeNull();
    }

    [Test]
    public void GameGeneratesATokenValueObject()
    {
        var game = new Game();
        var token = game.Join("P1");
        token.Should().BeOfType<PlayerToken>();
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
        game.StartGame();
        Assert.Throws<InvalidGameStateException>(() => game.Join("P2"));
    }

    [Test]
    public void CannotPlayIfYouHaveNotJoined()
    {
        var game = new Game();
        game.StartGame();
        var token1 = PlayerToken.Generate();
        Assert.Throws<UnrecognizedTokenException>(() => game.Move(token1, Direction.Forward));
    }

    [Test]
    public void PlayerCannotMoveIfGameStateIsNotPlaying()
    {
        var game = new Game();
        var token1 = game.Join("P1");
        Assert.Throws<InvalidGameStateException>(() => game.Move(token1, Direction.Forward));
    }

    [Test]
    public void WhenAPlayerJoinsTheyArePlacedOnTheMap()
    {
        var game = new Game();
        var token = game.Join("P1");
        var location = game.GetPlayerLocation(token);
        location.Should().NotBeNull();
    }

    [Test]
    public void TooManyPlayersCannotJoinGame()
    {
        var game = new Game(3, 3);
        foreach (var i in Enumerable.Range(0, 8))
        {
            game.Join($"P{i}");
        }
        Assert.Throws<TooManyPlayersException>(() => game.Join("P9"));
    }

    [TestCase(2, 2)]
    [TestCase(1, 2)]
    [TestCase(2, 1)]
    [TestCase(1, 1)]
    public void BoardMustBeAtLeast3x3(int rows, int cols)
    {
        Assert.Throws<BoardTooSmallException>(() => new Game(rows, cols));
    }
}
