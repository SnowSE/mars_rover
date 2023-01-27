namespace Mars.Web.Types;

public class JoinResponse
{
    public string Token { get; set; }
    public int StartingRow { get; set; }
    public int StartingColumn { get; set; }
    public Cell[] Neighbors { get; set; }
    public LowResolutionMapTile[] LowResolutionMap { get; set; }
}

public class LowResolutionMapTile
{
    public int LowerLeftRow { get; set; }
    public int LowerLeftColumn { get; set; }
    public int UpperRightRow { get; set; }
    public int UpperRightColumn { get; set; }
    public int AverageDifficulty { get; set; }
}
