namespace Mars.MissionControl.Tests;

public class BoardTests
{
    [Test]
    public void BoardIsInitializedWithARowAndColumn()
    {
        var map = Helpers.CreateMap(3, 3);
        var board = new Board(map);
        board.Cells.Count.Should().Be(9);
        board.Width.Should().Be(3);
        board.Height.Should().Be(3);
    }

    [Test]
    public void CanAccessCellsViaIndexer()
    {
        var map = Helpers.CreateMap(3, 3);
        var board = new Board(map);
        board[new Location(0, 0)].Should().Be(board.Cells[new Location(0, 0)]);
    }

    [Test]
    public void CanAccessCellsViaIndexerUsingIntegers()
    {
        var map = Helpers.CreateMap(3, 3);
        var board = new Board(map);
        board[0, 0].Should().Be(board.Cells[new Location(0, 0)]);
    }

    [Test]
    public void GetNeighborsInCornerReturnsEight()
    {
        var map = Helpers.CreateMap(3, 3);
        var board = new Board(map);
        var neighbors = board.GetNeighbors(new Location(0, 0), 2);
        neighbors.Should().BeEquivalentTo(new[]
        {
            board[0,0],
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
