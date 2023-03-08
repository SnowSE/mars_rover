namespace Mars.Web.Types;

public class JoinResponse
{
    public string Token { get; set; }
    public int StartingX { get; set; }
    public int StartingY { get; set; }
    public IEnumerable<Location> Targets { get; set; }
    public IEnumerable<Cell> Neighbors { get; set; }
    public IEnumerable<LowResolutionMapTile> LowResolutionMap { get; set; }
    public string Orientation { get; set; }
}

public record Location(int X, int Y);