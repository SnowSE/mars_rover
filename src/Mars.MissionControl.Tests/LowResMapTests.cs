using System.Linq;

namespace Mars.MissionControl.Tests;

internal class LowResMapTests
{
    [Test]
    public void Games10by10OrLessGetBackSingleTile()
    {
        var map = Helpers.CreateMap(5, 5);
        var game = Helpers.CreateGame(map);
        map.LowResolution.Count().Should().Be(1);
        map.LowResolution.First().AverageDifficulty.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.Difficulty.Value)
            );
    }

    [Test]
    public void Game20by20GetBackTwoTiles()
    {
        var map = Helpers.CreateMap(20, 10);
        var game = Helpers.CreateGame(map);
        map.LowResolution.Count().Should().Be(2);
        map.LowResolution.First().AverageDifficulty.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.Difficulty.Value)
            );
    }


    [Test]
    public void Game20by65GetBack14Tiles()
    {
        var map = Helpers.CreateMap(20, 65);
        var game = Helpers.CreateGame(map);
        map.LowResolution.Count().Should().Be(14);
        map.LowResolution.First().AverageDifficulty.Value
            .Should().Be(
               (int)game.Board.Cells.Average(c => c.Value.Difficulty.Value)
            );
    }
}


