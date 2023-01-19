namespace Mars.MissionControl.Tests;

internal class JoinGameTests
{
    [Test]
    public async Task CanJoinGame()
    {
        var game = new Game();
        var token = game.Join("Jonathan");
        token.Should().NotBeEmpty();
    }
}
