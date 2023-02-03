namespace Mars.MissionControl;

public static class Helpers
{
    public static Map CreateMap(int height, int width)
    {
        var cells = new List<Cell>();
        for (int row = 0; row < width; row++)
        {
            for (int col = 0; col < height; col++)
            {
                cells.Add(new Cell(new Location(row, col), new Difficulty(1)));
            }
        }
        var map = new Map(1, cells, BuildLowResMap(cells));
        return map;
    }

    public static Board CreateBoard(int height, int width)
    {
        var map = CreateMap(height, width);
        return new Board(map);
    }

    public static Game CreateGame(int height, int width)
    {
        var map = Helpers.CreateMap(5, 5);
        return CreateGame(map);
    }

    public static Game CreateGame(Map map)
    {
        var startOptions = new GameStartOptions
        {
            Map = map
        };
        var game = new Game(startOptions);
        return game;
    }

    public static List<LowResolutionCell> BuildLowResMap(List<Cell> cells)
    {
        var lowResTiles = new List<LowResolutionCell>();

        int height = cells.Max(c => c.Location.Column) + 1;
        int width = cells.Max(c => c.Location.Row) + 1;
        int tileSize = 10;

        for (int startingRow = 0; startingRow < width; startingRow += tileSize)
        {
            for (int startingCol = 0; startingCol < height; startingCol += tileSize)
            {
                bool isInChunk(Location l) =>
                    l.Row >= startingRow && l.Row < (startingRow + tileSize) &&
                    l.Column >= startingCol && l.Column < (startingCol + tileSize);

                var innerCells = cells
                    .Where(c => isInChunk(c.Location))
                    .Select(c => c);
                var newTile = new LowResolutionCell(innerCells);
                lowResTiles.Add(newTile);
            }
        }

        return lowResTiles;
    }
}
