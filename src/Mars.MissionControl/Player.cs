namespace Mars.MissionControl;

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
        PerseveranceLocation = new Location(0, 0);
    }

    public TimeSpan? WinningTime { get; set; }
    public long BatteryLevel { get; init; }
    public PlayerToken Token { get; private set; }
    public string Name { get; private set; }
    public Location PerseveranceLocation { get; init; }
    public Orientation Orientation { get; init; }
}
