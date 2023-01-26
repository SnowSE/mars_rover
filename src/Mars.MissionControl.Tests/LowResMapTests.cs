using System.Linq;

namespace Mars.MissionControl.Tests;

internal class LowResMapTests
{
    [Test]
    public void Games10by10OrLessGetBackSingleTile()
    {
        var game = new Game(10, 10);
        var map = new Map(game);
        map.LowResolution.Count().Should().Be(1);
        map.LowResolution.First().DamageValue.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.DamageValue.Value)
            );
    }
}


