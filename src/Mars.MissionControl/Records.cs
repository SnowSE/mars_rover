namespace Mars.MissionControl;

public record Location(int X, int Y);
public record Cell(Location Location, Difficulty Difficulty);
public record JoinResult(PlayerToken Token, Location PlayerLocation, Orientation Orientation, int BatteryLevel, Location TargetLocation, IEnumerable<Cell> Neighbors, IEnumerable<LowResolutionCell> LowResolutionMap);
public record MoveResult(Location Location, int BatteryLevel, Orientation Orientation, IEnumerable<Cell> Neighbors, string Message);
public record IngenuityMoveResult(Location Location, int BatteryLevel, IEnumerable<Cell> Neighbors, string Message);

public enum Orientation
{
    North,
    East,
    South,
    West
}

public enum GameState
{
    Joining,
    Playing,
    GameOver
}

public enum Direction
{
    Forward,
    Left,
    Right,
    Reverse
}