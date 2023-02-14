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
        IngenuityLocation = new Location(0, 0);
    }

    public TimeSpan? WinningTime { get; set; }
    public int BatteryLevel { get; init; }
    public PlayerToken Token { get; private set; }
    public string Name { get; private set; }
    public Location PerseveranceLocation { get; init; }
    public Location IngenuityLocation { get; init; }
    public int IngenuityBatteryLevel { get; init; }
    public Orientation Orientation { get; init; }
}
