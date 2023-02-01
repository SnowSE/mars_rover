namespace Mars.Web.Tests;

internal class ComponentTests
{
    [Test]
    [Ignore("need to fix...")]
    public void GameStatusShowsJoining()
    {
        using var ctx = new Bunit.TestContext();

        //var cut = ctx.Render(@"<CascadingValue Name = ""gameManager"" Value = @(gameManager) >
        //    < GameStatus />
        //    </ CascadingValue >");
        //cut.Markup.Should().Contain("Joining");
    }
}
