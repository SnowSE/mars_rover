using System;
using System.Linq;

namespace Mars.MissionControl.Tests;

public class BatteryTests
{

    [Test]
    public async Task BatteryCharges()
    {
        var scenario = new GameScenario(height: 7, width: 7, players: 1, customLogger: TestLogger.MakeNewGameLogger());
        scenario.Game.PlayGame();
        var player1Driver = new InsanelySimpleRoverDriver(scenario.Players[0], scenario.Game);
        player1Driver.MoveTowardTarget(2);
        var previousBatteryLevel = scenario.Players[0].BatteryLevel;

        await Task.Delay(TimeSpan.FromMilliseconds(1_100));//wait for a charge

        scenario.Game.Players[0].BatteryLevel.Should().BeGreaterThan(previousBatteryLevel);
    }

    [Test]
    public void WhenAllBatteriesAreAtZeroThenEveryoneGetsABoost()
    {
        var scenario = new GameScenario(height: 10, width: 10, players: 2, cellDifficulty: 1000, startingBatteryLevel: 3000, TestLogger.MakeNewGameLogger());
        scenario.Game.PlayGame(new GamePlayOptions { RechargePointsPerSecond = 0 });
        var player1Driver = new InsanelySimpleRoverDriver(scenario.Players[0], scenario.Game);
        player1Driver.DriveUntilBatteryDies();

        scenario.Game.Players.First(p => p.Token == scenario.Players[0].Token).BatteryLevel.Should().BeLessThan(100);

        var player2Driver = new InsanelySimpleRoverDriver(scenario.Players[1], scenario.Game);
        player2Driver.DriveUntilBatteryDies();

        scenario.Game.Players.First(p => p.Token == scenario.Players[0].Token).BatteryLevel.Should().BeGreaterThan(100);
    }
}
