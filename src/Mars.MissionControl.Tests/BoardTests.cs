using System.Linq;

namespace Mars.MissionControl.Tests;

public class BoardTests
{
    [Test]
    public void BoardIsInitializedWithARowAndColumn()
    {
        var board = new Board(2, 2);
        board.Cells.Count().Should().Be(4);
    }

    //var cell = board[location]
    //var cell = board[row, col]
}
