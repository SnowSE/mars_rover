namespace Mars.MissionControl;

public record Location(int Row, int Column);

public record Cell(Location Location, Difficulty Difficulty);

public record Difficulty
{
    public Difficulty(int value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Value = value;
    }

    public int Value { get; init; }
}

public record Player
{
    public Player(string name)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        Token = PlayerToken.Generate();
        Location = new Location(0, 0);
    }

    public int BatteryLevel { get; init; }
    public PlayerToken Token { get; private set; }
    public string Name { get; private set; }
    public Location Location { get; init; }
    public Orientation Orientation { get; init; }
}

public enum Orientation { North, East, South, West }

public record LowResolutionCell
{
    public LowResolutionCell(IEnumerable<Cell> cells)
    {
        AverageDifficulty = new Difficulty((int)cells.Average(c => c.Difficulty.Value));
        LowerLeftColumn = cells.Min(c => c.Location.Column);
        LowerLeftRow = cells.Min(c => c.Location.Row);
        UpperRightColumn = cells.Max(c => c.Location.Column);
        UpperRightRow = cells.Max(c => c.Location.Row);
    }

    public Difficulty AverageDifficulty { get; private set; }
    public int LowerLeftRow { get; private set; }
    public int LowerLeftColumn { get; private set; }
    public int UpperRightRow { get; private set; }
    public int UpperRightColumn { get; private set; }
}
