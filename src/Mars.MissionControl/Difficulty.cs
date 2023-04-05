namespace Mars.MissionControl;

public record Difficulty
{
    public Difficulty(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Value = value;
    }

    public int Value { get; init; }
}
