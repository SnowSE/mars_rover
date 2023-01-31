namespace Mars.MissionControl;

public static class PlayerExtensions
{
    public static Location CellInFront(this Player p)
    {
        return p.Orientation switch
        {
            Orientation.North => p.Location with { Column = p.Location.Column + 1 },
            Orientation.East => p.Location with { Row = p.Location.Row + 1 },
            Orientation.South => p.Location with { Column = p.Location.Column - 1 },
            Orientation.West => p.Location with { Row = p.Location.Row - 1 },
            _ => throw new Exception("Invalid orientation")
        };
    }

    public static Location CellInBack(this Player p)
    {
        return p.Orientation switch
        {
            Orientation.North => p.Location with { Column = p.Location.Column - 1 },
            Orientation.East => p.Location with { Row = p.Location.Row - 1 },
            Orientation.South => p.Location with { Column = p.Location.Column + 1 },
            Orientation.West => p.Location with { Row = p.Location.Row + 1 },
            _ => throw new Exception("Invalid orientation")
        };
    }

    public static Orientation Turn(this Orientation orientation, Direction direction)
    {
        if (direction == Direction.Forward || direction == Direction.Reverse)
            throw new ArgumentOutOfRangeException(nameof(Direction), "You can only turn right or left");

        int clockwise = (direction == Direction.Right) ? 1 : 7;
        var newOrientation = (Orientation)(((int)orientation + clockwise) % 4);
        return newOrientation;
    }
}
