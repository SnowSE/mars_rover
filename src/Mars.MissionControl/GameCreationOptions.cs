namespace Mars.MissionControl;

public class GameCreationOptions
{
    private int perseveranceVisibilityRadius = 2;
    private int ingenuityVisibilityRadius = 5;
    private int startingBatteryLevel = 18_000;
    private int numberOfIngenuitiesPerPlayer = 10;
    private int minimumBatteryThreshhold = 1_000;
    private int keepTheGameGoingBatteryBoostAmount = 2_000;

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

    public int MinimumBatteryThreshhold
    {
        get => minimumBatteryThreshhold;
        set
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MinimumBatteryThreshhold), "Must be > 0");
            }

            minimumBatteryThreshhold = value;
        }
    }

    public int KeepTheGameGoingBatteryBoostAmount
    {
        get => keepTheGameGoingBatteryBoostAmount;
        set
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(KeepTheGameGoingBatteryBoostAmount), "Must be > 0");
            }

            keepTheGameGoingBatteryBoostAmount = value;
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


    public int NumberOfIngenuitiesPerPlayer
    {
        get => numberOfIngenuitiesPerPlayer;
        set
        {
            if (value < 1 || value > 99)
            {
                throw new ArgumentOutOfRangeException(nameof(NumberOfIngenuitiesPerPlayer), "Must be between 1 and 99");
            }

            numberOfIngenuitiesPerPlayer = value;
        }
    }

    public MapWithTargets MapWithTargets { get; set; }

    public override string ToString() =>
        $"Map#={MapWithTargets.Map.MapNumber}; BatteryLevel={StartingBatteryLevel}; PerseveranceVisibility={PerseveranceVisibilityRadius}, IngenuityVisibility={IngenuityVisibilityRadius}";
}
