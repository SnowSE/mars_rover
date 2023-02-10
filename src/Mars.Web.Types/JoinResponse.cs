namespace Mars.Web.Types;

public class JoinResponse
{
    public string Token { get; set; }
    public int StartingX { get; set; }
    public int StartingY { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public IEnumerable<Cell> Neighbors { get; set; }
    public IEnumerable<LowResolutionMapTile> LowResolutionMap { get; set; }
    public string Orientation { get; set; }
}
