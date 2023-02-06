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

    public override bool Equals(object? obj)
    {
        return obj is Map map &&
               Height == map.Height &&
               Width == map.Width &&
               EqualityComparer<IEnumerable<LowResolutionCell>>.Default.Equals(LowResolution, map.LowResolution) &&
               MapNumber == map.MapNumber &&
               EqualityComparer<IEnumerable<Cell>>.Default.Equals(HighResolution, map.HighResolution);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Height, Width, LowResolution, MapNumber, HighResolution);
    }
}
