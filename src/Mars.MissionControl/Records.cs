namespace Mars.MissionControl;

public record Location(int Row, int Column);

public record Cell(Location Location, DamageValue DamageValue, Player? Occupant);

public record DamageValue
{
    public DamageValue(int value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentOutOfRangeException();
        }

        Value = value;
    }

    public int Value { get; set; }
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
        DamageValue = new DamageValue((int)cells.Average(c => c.DamageValue.Value));
    }
    public DamageValue DamageValue { get; private set; }
}

