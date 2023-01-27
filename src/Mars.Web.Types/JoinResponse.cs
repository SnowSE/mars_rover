namespace Mars.Web.Types;

public class JoinResponse
{
    public string Token { get; set; }
    public int StartingRow { get; set; }
    public int StartingColumn { get; set; }
    public IEnumerable<Cell> Neighbors { get; set; }
    public IEnumerable<LowResolutionMapTile> LowResolutionMap { get; set; }
}
