using System.Linq;

namespace Mars.MissionControl.Tests;

public class WinTheGameTests
{
    private GameScenario scenario;
    private InsanelySimpleRoverDriver player1Driver;

    [SetUp]
    public void Setup()
    {
        scenario = new GameScenario(
            height: 10,
            width: 10,
            players: 1,
            customLogger: TestLogger.MakeNewGameLogger(),
            targets: new[] { new Location(2, 2), new Location(3, 3), new Location(5, 5) });
        scenario.Game.PlayGame(new GamePlayOptions { RechargePointsPerSecond = 0 });
        player1Driver = new InsanelySimpleRoverDriver(scenario.Players[0], scenario.Game);
    }

    [Test]
    public void GoingToTheLastTargetFirstDoesntCount()
    {
        var lastTarget = scenario.Game.Targets.Last();
        player1Driver.MoveTo(lastTarget);
        Assert.That(player1Driver.CurrentLocation == lastTarget);

        var currentPlayerState = scenario.Game.Players.Single(p => p.Token == scenario.Players[0].Token);
        player1Driver.LastMoveMessage.Should().NotBe(GameMessages.YouMadeItToTheTarget);
    }

    [Test]
    public void GoingToTheFirstTargetFirstCounts()
    {
        var firstTarget = scenario.Game.Targets.First();
        player1Driver.MoveTo(firstTarget);
        Assert.That(player1Driver.CurrentLocation == firstTarget);

        var currentPlayerState = scenario.Game.Players.Single(p => p.Token == scenario.Players[0].Token);
        player1Driver.LastMoveMessage.Should().Be(GameMessages.YouMadeItToTheTarget);
    }

    [Test]
    public void GoingToAllTargetsInOrderWorks()
    {
        player1Driver.DriveUntilBatteryDies();
        Assert.That(scenario.Game.Winners.Any(w => w.Token == scenario.Players[0].Token));
    }
}
