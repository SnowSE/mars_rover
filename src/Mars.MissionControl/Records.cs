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
    }

    public PlayerToken Token { get; private set; }
    public string Name { get; private set; }
}

public record LowResolutionCell
{
    public LowResolutionCell(IEnumerable<Cell> cells)
    {
        Difficulty = new Difficulty((int)cells.Average(c => c.Difficulty.Value));
    }
    public Difficulty Difficulty { get; private set; }
}
