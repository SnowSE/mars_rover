namespace Mars.Web.Types;

public class IngenuityMoveResponse
{
    public int X { get; set; }
    public int Y { get; set; }
    public int BatteryLevel { get; set; }
    public IEnumerable<Cell> Neighbors { get; set; }
    public string Message { get; set; }
}
