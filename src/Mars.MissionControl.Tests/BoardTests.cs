namespace Mars.MissionControl.Tests;

public class BoardTests
{
    [Test]
    public void BoardIsInitializedWithARowAndColumn()
    {
        var board = new Board(2, 2);
        board.Cells.Count.Should().Be(4);
        board.Width.Should().Be(2);
        board.Height.Should().Be(2);
    }

    [Test]
    public void CanAccessCellsViaIndexer()
    {
        var board = new Board(2, 2);
        board[new Location(0, 0)].Should().Be(board.Cells[new Location(0, 0)]);
    }

    [Test]
    public void CanAccessCellsViaIndexerUsingIntegers()
    {
        var board = new Board(2, 2);
        board[0, 0].Should().Be(board.Cells[new Location(0, 0)]);
    }

    [Test]
    public void PlacePlayer()
    {
        var board = new Board(5, 5);
        var player = new Player("P1");
        Location location = board.PlaceNewPlayer(player);
        board.RoverLocations[player].Should().Be(location);
    }

    [Test]
    public void GetNeighborsInCornerReturnsThree()
    {
        var board = new Board(5, 5);
        var neighbors = board.GetNeighbors(new Location(0, 0), 2);
        neighbors.Should().BeEquivalentTo(new[]
        {
            board[1,0],
            board[1,1],
            board[0,1],
            board[2,0],
            board[2,1],
            board[2,2],
            board[1,2],
            board[0,2],
        });
    }
}
