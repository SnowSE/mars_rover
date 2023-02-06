namespace Mars.MissionControl;

public record LowResolutionCell
{
    public LowResolutionCell(IEnumerable<Cell> cells)
    {
        AverageDifficulty = new Difficulty((int)cells.Average(c => c.Difficulty.Value));
        LowerLeftColumn = cells.Min(c => c.Location.Column);
        LowerLeftRow = cells.Min(c => c.Location.Row);
        UpperRightColumn = cells.Max(c => c.Location.Column);
        UpperRightRow = cells.Max(c => c.Location.Row);
    }

    public LowResolutionCell(int averageDifficulty, int lowerLeftRow, int lowerLeftColumn, int upperRightRow, int upperRightColumn)
    {
        AverageDifficulty = new Difficulty(averageDifficulty);
        LowerLeftColumn = lowerLeftColumn;
        LowerLeftRow = lowerLeftRow;
        UpperRightColumn = upperRightColumn;
        UpperRightRow = upperRightRow;
    }

    public Difficulty AverageDifficulty { get; private set; }
    public int LowerLeftRow { get; private set; }
    public int LowerLeftColumn { get; private set; }
    public int UpperRightRow { get; private set; }
    public int UpperRightColumn { get; private set; }
}
