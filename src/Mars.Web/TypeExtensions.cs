namespace Mars.Web;

public static class TypeExtensions
{
    public static IEnumerable<Types.Cell> ToDto(this IEnumerable<MissionControl.Cell> cells) =>
        cells.Select(c => new Types.Cell()
        {
            Y = c.Location.Y,
            X = c.Location.X,
            Difficulty = c.Difficulty.Value
        });

    public static IEnumerable<Types.LowResolutionMapTile> ToDto(this IEnumerable<LowResolutionCell> cells) =>
        cells.Select(t => new LowResolutionMapTile
        {
            AverageDifficulty = t.AverageDifficulty.Value,
            LowerLeftX = t.LowerLeftX,
            LowerLeftY = t.LowerLeftY,
            UpperRightY = t.UpperRightY,
            UpperRightX = t.UpperRightX
        });

    public static IEnumerable<Types.Location> ToDto(this IEnumerable<MissionControl.Location> locations) => locations.Select(l => new Types.Location(l.X, l.Y));
}