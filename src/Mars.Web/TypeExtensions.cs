namespace Mars.Web;

public static class TypeExtensions
{
    public static IEnumerable<Types.Cell> ToDto(this IEnumerable<MissionControl.Cell> cells) => 
        cells.Select(c => new Types.Cell()
        {
            Column = c.Location.Column,
            Row = c.Location.Row,
            Difficulty = c.Difficulty.Value
        });

    public static IEnumerable<Types.LowResolutionMapTile> ToDto(this IEnumerable<LowResolutionCell> cells) =>
        cells.Select(t => new LowResolutionMapTile
        {
            AverageDifficulty = t.AverageDifficulty.Value,
            LowerLeftRow = t.LowerLeftRow,
            LowerLeftColumn = t.LowerLeftColumn,
            UpperRightColumn = t.UpperRightColumn,
            UpperRightRow = t.UpperRightRow
        });
}