namespace Mars.MissionControl;

public static class PlayerExtensions
{
    public static Location CellInFront(this Player p)
    {
        return p.Orientation switch
        {
            Orientation.North => p.PerseveranceLocation with { Y = p.PerseveranceLocation.Y + 1 },
            Orientation.East => p.PerseveranceLocation with { X = p.PerseveranceLocation.X + 1 },
            Orientation.South => p.PerseveranceLocation with { Y = p.PerseveranceLocation.Y - 1 },
            Orientation.West => p.PerseveranceLocation with { X = p.PerseveranceLocation.X - 1 },
            _ => throw new Exception("Invalid orientation")
        };
    }

    public static Location CellInBack(this Player p)
    {
        return p.Orientation switch
        {
            Orientation.North => p.PerseveranceLocation with { Y = p.PerseveranceLocation.Y - 1 },
            Orientation.East => p.PerseveranceLocation with { X = p.PerseveranceLocation.X - 1 },
            Orientation.South => p.PerseveranceLocation with { Y = p.PerseveranceLocation.Y + 1 },
            Orientation.West => p.PerseveranceLocation with { X = p.PerseveranceLocation.X + 1 },
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
