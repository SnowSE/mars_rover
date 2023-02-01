namespace Mars.Web;

public static class TypeExtensions
{
    public static IEnumerable<Types.Cell> ToDto(this IEnumerable<MissionControl.Cell> cells)
    {
        return cells.Select(c => new Types.Cell()
        {
            Column = c.Location.Column,
            Row = c.Location.Row,
            Difficulty = c.Difficulty.Value
        });
    }
}