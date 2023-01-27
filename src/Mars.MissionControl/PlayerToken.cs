namespace Mars.MissionControl;

public class PlayerToken : IEquatable<PlayerToken?>
{
    public string Value { get; init; }

    private PlayerToken(string value)
    {
        Value = value;
    }

    public static PlayerToken Generate()
    {
        return new PlayerToken(IdGenerator.GetNextId());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PlayerToken);
    }

    public bool Equals(PlayerToken? other)
    {
        return other is not null &&
               Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(PlayerToken? left, PlayerToken? right)
    {
        return EqualityComparer<PlayerToken>.Default.Equals(left, right);
    }

    public static bool operator !=(PlayerToken? left, PlayerToken? right)
    {
        return !(left == right);
    }
}