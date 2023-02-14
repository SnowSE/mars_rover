namespace Mars.MissionControl;

public class GamePlayOptions
{
    public int RechargePointsPerSecond { get; set; } = 10;

    public override bool Equals(object? obj)
    {
        return obj is GamePlayOptions options &&
               RechargePointsPerSecond == options.RechargePointsPerSecond;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RechargePointsPerSecond);
    }
}