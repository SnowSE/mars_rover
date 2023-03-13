namespace Mars.MissionControl.Tests;

public class MapWithTargetsTests
{
    [Test]
    public void ValidTargets()
    {
        var actualLocations = MapWithTargets.ParseLocations("(200,200);(250,250)");
        actualLocations.Should().BeEquivalentTo(new[] { new Location(200, 200), new Location(250, 250) });
    }

    [Test]
    public void InvalidTargets()
    {
        var action = () => MapWithTargets.ParseLocations("200;250");
        action.Should().Throw<InvalidFormatException>();
    }
}
