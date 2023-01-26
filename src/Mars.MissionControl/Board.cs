using System.Collections.Concurrent;

namespace Mars.MissionControl;

public class Board
{
    public Board(int numRows, int numColumns)
    {
        Width = numRows;
        Height = numColumns;
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
                var newCell = new Cell(new Location(row, col), new DamageValue(0), null);
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
        Location location;
        if (!startingLocations.TryDequeue(out location))
        {
            throw new TooManyPlayersException();
        }

        var origCell = Cells[location];
        var newCell = origCell with { Occupant = player };
        if (!Cells.TryUpdate(location, newCell, origCell))
        {
            throw new UnableToUpdateBoardException();
        }

        return location;
    }

    public Location FindPlayer(Player player)
    {
        return Cells.FirstOrDefault(kvp => kvp.Value.Occupant == player).Key;
    }
    public Location FindPlayer(PlayerToken token)
    {
        var location = new Location(1, 2);
        var newLocation = location with { Column = 5 };
        return Cells.FirstOrDefault(kvp => kvp.Value.Occupant?.Token == token).Key;
    }
}

public record Location(int Row, int Column);

public record Cell(Location Location, DamageValue DamageValue, Player? Occupant);

public record DamageValue
{
    public DamageValue(int value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentOutOfRangeException();
        }

        Value = value;
    }

    public int Value { get; set; }
}

public record Player
{
    public Player(string name)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        Token = PlayerToken.Generate();
    }

    public PlayerToken Token { get; private set; }
    public string Name { get; private set; }
}
