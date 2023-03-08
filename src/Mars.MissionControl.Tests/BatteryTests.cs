using System;

namespace Mars.MissionControl.Tests;

public class BatteryTests
{
    [Test]
    public async Task BatteryCharges()
    {
        var scenario = new GameScenario(height: 7, width: 7, players: 1);
        scenario.Game.PlayGame();
        scenario.Game.MovePerseverance(scenario.Players[0].Token, Direction.Right);
        scenario.Game.Players[0].BatteryLevel.Should().Be(scenario.Game.StartingBatteryLevel - 1);
        await Task.Delay(TimeSpan.FromMilliseconds(1_100));
        scenario.Game.Players[0].BatteryLevel.Should().Be(scenario.Game.StartingBatteryLevel + 9);
    }
}
