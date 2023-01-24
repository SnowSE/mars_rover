using System.Collections.Concurrent;

namespace Mars.MissionControl;

public class Board
{
    public Board(int numRows, int numColumns)
    {
        Cells = new ConcurrentDictionary<Location, Cell>();

    }

    public ConcurrentDictionary<Location, Cell> Cells { get; init; }
}

public record Location(int Row, int Column);

public record Cell(Location Location, DamageValue DamageValue, Player? Occupant);

public record DamageValue
{
    public DamageValue(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException()
}
    }
}
