namespace Mars.MissionControl.Tests;

internal class JoinGameTests
{
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
}
