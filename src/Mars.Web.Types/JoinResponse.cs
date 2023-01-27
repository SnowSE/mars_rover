namespace Mars.Web.Types;

public class JoinResponse
{
    public string Token { get; set; }
    public int StartingRow { get; set; }
    public int StartingColumn { get; set; }
    public Cell[] Neighbors { get; set; }
}
