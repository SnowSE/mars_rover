namespace Mars.MissionControl;

public class GameStartOptions
{
    private int perseveranceVisibilityRadius = 2;
    private int ingenuityVisibilityRadius = 5;
    private int startingBatteryLevel = 18_000;

    public IEnumerable<Location> Targets { get; set; }

    public int StartingBatteryLevel
    {
        get => startingBatteryLevel;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(StartingBatteryLevel), "Must be > 0");
            }

            startingBatteryLevel = value;
        }
    }

    public int PerseveranceVisibilityRadius
    {
        get => perseveranceVisibilityRadius;
        set
        {
            if (value < 1 || value > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(PerseveranceVisibilityRadius), "Must be between 1 and 15.");
            }

            perseveranceVisibilityRadius = value;
        }
    }

    public int IngenuityVisibilityRadius
    {
        get => ingenuityVisibilityRadius;
        set
        {
            if (value < 1 || value > 25)
            {
                throw new ArgumentOutOfRangeException(nameof(IngenuityVisibilityRadius), "Must be between 1 and 25");
            }

            ingenuityVisibilityRadius = value;
        }
    }

    public Map Map { get; set; }

    public override string ToString() =>
        $"Map#={Map.MapNumber}; BatteryLevel={StartingBatteryLevel}; PerseveranceVisibility={PerseveranceVisibilityRadius}, IngenuityVisibility={IngenuityVisibilityRadius}";
}
