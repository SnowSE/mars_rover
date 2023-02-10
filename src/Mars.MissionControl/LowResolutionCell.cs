namespace Mars.MissionControl;

public record LowResolutionCell
{
    public LowResolutionCell(IEnumerable<Cell> cells)
    {
        AverageDifficulty = new Difficulty((int)cells.Average(c => c.Difficulty.Value));
        LowerLeftY = cells.Min(c => c.Location.Y);
        LowerLeftX = cells.Min(c => c.Location.X);
        UpperRightY = cells.Max(c => c.Location.Y);
        UpperRightX = cells.Max(c => c.Location.X);
    }

    public LowResolutionCell(int averageDifficulty, int lowerLeftRow, int lowerLeftColumn, int upperRightRow, int upperRightColumn)
    {
        AverageDifficulty = new Difficulty(averageDifficulty);
        LowerLeftY = lowerLeftColumn;
        LowerLeftX = lowerLeftRow;
        UpperRightY = upperRightColumn;
        UpperRightX = upperRightRow;
    }

    public Difficulty AverageDifficulty { get; private set; }
    public int LowerLeftX { get; private set; }
    public int LowerLeftY { get; private set; }
    public int UpperRightX { get; private set; }
    public int UpperRightY { get; private set; }
}
