namespace Mars.MissionControl;

public class Board
{
    public Board(int numRows, int numColumns, int mapNumber = 1)
    {
        Width = numRows;
        Height = numColumns;
        MapNumber = mapNumber;
        Cells = new ConcurrentDictionary<Location, Cell>();
        initializeCells();
        initializeStartingLocations();
    }

    private void initializeCells()
    {
        foreach (var row in Enumerable.Range(0, Width))
        {
            foreach (var col in Enumerable.Range(0, Height))
            {
                var newCell = new Cell(new Location(row, col), new Difficulty(1));
                if (!Cells.TryAdd(newCell.Location, newCell))
                {
                    throw new UnableToGenerateBoardException();
                }
            }
        }
    }

    private void initializeStartingLocations()
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

        startingLocations = new(locations.OrderBy(_ => Random.Shared.Next()));
    }

    public int MapNumber { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public ConcurrentDictionary<Location, Cell> Cells { get; init; }
    private ConcurrentQueue<Location> startingLocations;

    public ConcurrentDictionary<Player, Location> RoverLocations { get; init; } = new();

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
        Location location;
        if (!startingLocations.TryDequeue(out location))
        {
            throw new TooManyPlayersException();
        }

        RoverLocations.TryAdd(player, location);

        return location;
    }

    public IEnumerable<Cell> GetNeighbors(Location location, int numberOfCellsOnEachSide)
    {
        var width = numberOfCellsOnEachSide;
        var minRow = location.Row - width;
        var maxRow = location.Row + width;
        var minCol = location.Column - width;
        var maxCol = location.Column + width;

        var neighbors = from c in Cells
                        let l = c.Value.Location
                        where (l.Row >= minRow && l.Row <= maxRow && l.Column >= minCol && l.Column <= maxCol)
                        select c.Value;
        return neighbors;
    }
}
