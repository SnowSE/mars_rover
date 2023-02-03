namespace Mars.MissionControl;

public class Map
{
    public Map(int mapNumber, IEnumerable<Cell> highResMap, IEnumerable<LowResolutionCell> lowResMap)
    {
        MapNumber = mapNumber;
        HighResolution = highResMap;
        LowResolution = lowResMap;

        Height = highResMap.Max(c => c.Location.Column) + 1;
        Width = highResMap.Max(c => c.Location.Row) + 1;

        if (Width < 3 || Height < 3)
        {
            throw new BoardTooSmallException();
        }
    }

    public int Height { get; }
    public int Width { get; }

    public IEnumerable<LowResolutionCell> LowResolution { get; private set; }
    public int MapNumber { get; }
    public IEnumerable<Cell> HighResolution { get; }
}
