namespace Mars.Web.Tests;

internal class ComponentTests
{
    [Test]
    public void GameStatusShowsJoining()
    {
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<GameManager>();
        var cut = ctx.RenderComponent<Components.GameStatus>();
        cut.Markup.Should().Contain("Joining");
    }
}
