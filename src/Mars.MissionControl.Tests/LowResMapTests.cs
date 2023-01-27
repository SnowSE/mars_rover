using System.Linq;

namespace Mars.MissionControl.Tests;

internal class LowResMapTests
{
    [Test]
    public void Games10by10OrLessGetBackSingleTile()
    {
        var game = new Game(5, 5);
        var map = new Map(game);
        map.LowResolution.Count().Should().Be(1);
        map.LowResolution.First().AverageDifficulty.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.Difficulty.Value)
            );
    }

    [Test]
    public void Game20by20GetBackTwoTiles()
    {
        var game = new Game(20, 10);
        var map = new Map(game);
        map.LowResolution.Count().Should().Be(2);
        map.LowResolution.First().AverageDifficulty.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.Difficulty.Value)
            );
    }


    [Test]
    public void Game20by65GetBack14Tiles()
    {
        var game = new Game(20, 65);
        var map = new Map(game);
        map.LowResolution.Count().Should().Be(14);
        map.LowResolution.First().AverageDifficulty.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.Difficulty.Value)
            );
    }
}


