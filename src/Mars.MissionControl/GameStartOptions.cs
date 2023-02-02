namespace Mars.MissionControl;

public class GameStartOptions
{
    private int perseveranceVisibilityRadius = 2;
    private int ingenuityVisibilityRadius = 5;
    private int width = 100;
    private int height = 100;
    private int startingBatteryLevel = 100;

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

    public int Height
    {
        get => height;
        set
        {
            if (value < 5)
            {
                throw new ArgumentOutOfRangeException(nameof(Height), "Must be 5 or greater.");
            }

            height = value;
        }
    }

    public int Width
    {
        get => width;
        set
        {
            if (value < 5)
            {
                throw new ArgumentOutOfRangeException(nameof(Width), "Must be 5 or greater.");
            }

            width = value;
        }
    }

    public int MapNumber { get; set; } = 1;

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
}
