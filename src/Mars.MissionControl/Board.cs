namespace Mars.MissionControl;

public class Board
{
    public Board(Map map)
    {
        Width = map.Width;
        Height = map.Height;
        MapNumber = map.MapNumber;
        Cells = new ConcurrentDictionary<Location, Cell>(map.HighResolution.Select(c => new KeyValuePair<Location, Cell>(c.Location, c)));
        startingLocations = initializeStartingLocations();
    }

    private ConcurrentQueue<Location> initializeStartingLocations()
    {
        var locations = new List<Location>();
        for (int i = 0; i < Width; i++)
        {
            locations.Add(new Location(i, 0));
            locations.Add(new Location(i, Height - 1));
        }

        for (int i = 1; i < Height - 1; i++)
        {
            locations.Add(new Location(0, i));
            locations.Add(new Location(Width - 1, i));
        }

        return new(locations.OrderBy(_ => Random.Shared.Next()));
    }

    public int MapNumber { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public ConcurrentDictionary<Location, Cell> Cells { get; init; }
    private ConcurrentQueue<Location> startingLocations;

    public Cell this[Location location]
    {
        get => Cells[location];
        set
        {
            if (!Cells.TryUpdate(location, value, Cells[location]))
            {
                throw new UnableToUpdateBoardException();
            }
        }
    }

    public Cell this[int row, int col]
    {
        get => Cells[new Location(row, col)];
        set
        {
            var location = new Location(row, col);
            if (!Cells.TryUpdate(location, value, Cells[location]))
            {
                throw new UnableToUpdateBoardException();
            }
        }
    }

    public Location PlaceNewPlayer(Player player)
    {
        Location? location;
        if (!startingLocations.TryDequeue(out location))
        {
            throw new TooManyPlayersException();
        }

        return location;
    }

    public IEnumerable<Cell> GetNeighbors(Location location, int numberOfCellsOnEachSide)
    {
        var width = numberOfCellsOnEachSide;
        var minRow = location.X - width;
        var maxRow = location.X + width;
        var minCol = location.Y - width;
        var maxCol = location.Y + width;

        var neighbors = from c in Cells
                        let l = c.Value.Location
                        where (l.X >= minRow && l.X <= maxRow && l.Y >= minCol && l.Y <= maxCol)
                        select c.Value;
        return neighbors;
    }
}
