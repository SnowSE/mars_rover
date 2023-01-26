using System;

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
        board.FindPlayer(player).Should().Be(location);
    }

    [Test]
    public void CanMovePlayerOnce()
    {
        var board = new Board(5, 5);
        var player = new Player("P1");
        Location initialLocation = board.PlaceNewPlayer(player);
        var newLocation = board.MovePlayer(player, Direction.Forward);
        newLocation.Should().Be(new Location(initialLocation.Row, initialLocation.Column + 1));
        board.FindPlayer(player).Should().Be(new Location(0, 1));
    }

    [Test]
    public void CanMovePlayerMultiple()
    {
        var board = new Board(5, 5);
        var player = new Player("P1");
        Location initialLocation = board.PlaceNewPlayer(player);
        board.MovePlayer(player, Direction.Forward);
        board.FindPlayer(player).Should().Be(new Location(initialLocation.Row, initialLocation.Column + 1));
        board.MovePlayer(player, Direction.Right);
        board.FindPlayer(player).Should().Be(new Location(initialLocation.Row + 1, initialLocation.Column + 1));
        board.MovePlayer(player, Direction.Reverse);
        board.FindPlayer(player).Should().Be(new Location(initialLocation.Row + 1, initialLocation.Column));
        board.MovePlayer(player, Direction.Left);
        board.FindPlayer(player).Should().Be(initialLocation);
    }

    [Test]
    public void MovingPlayerOffBoardIsInvalidMove()
    {
        var board = new Board(5, 5);
        var player = new Player("P1");
        Location initialLocation = board.PlaceNewPlayer(player);
        //Move to edge of board
        while (board.FindPlayer(player).Column > 0)
        {
            board.MovePlayer(player, Direction.Reverse);
        }
        //Move off board
        Assert.Throws<InvalidDirectionException>(() => board.MovePlayer(player, Direction.Reverse));
    }
}
