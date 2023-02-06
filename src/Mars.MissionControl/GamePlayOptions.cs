namespace Mars.MissionControl;

public class GamePlayOptions
{
    public int MaxPlayerMessagesPerSecond { get; set; } = 3;
    public int RechargePointsPerSecond { get; set; } = 10;

    public override bool Equals(object? obj)
    {
        return obj is GamePlayOptions options &&
               MaxPlayerMessagesPerSecond == options.MaxPlayerMessagesPerSecond &&
               RechargePointsPerSecond == options.RechargePointsPerSecond;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MaxPlayerMessagesPerSecond, RechargePointsPerSecond);
    }
}