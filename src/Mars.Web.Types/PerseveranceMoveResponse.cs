namespace Mars.Web.Types;

public class PerseveranceMoveResponse
{
    public int X { get; set; }
    public int Y { get; set; }
    public long BatteryLevel { get; set; }
    public IEnumerable<Cell> Neighbors { get; set; }
    public string Message { get; set; }
    public string Orientation { get; set; }
}
