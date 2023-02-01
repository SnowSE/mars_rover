namespace Mars.Web.Types;

public class MoveResponse
{
    public int Row { get; set; }
    public int Column { get; set; }
    public int BatteryLevel { get; set; }
    public IEnumerable<Cell> Neighbors { get; set; }
    public string Message { get; set; }
}
