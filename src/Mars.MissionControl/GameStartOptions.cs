namespace Mars.MissionControl;

public class GameStartOptions
{
    private int perseveranceVisibilityRadius = 2;
    private int ingenuityVisibilityRadius = 5;
    private int width = 100;
    private int height = 100;
    private int startingBatteryLevel = 2_000;

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
}
