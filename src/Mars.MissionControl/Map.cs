namespace Mars.MissionControl;

public class Map
{
    public const int TileSize = 10;
    private readonly Game game;

    public Map(Game game)
    {
        this.game = game;
        fillLowResoulutionMap();
    }

    public IEnumerable<LowResolutionCell> LowResolution { get; private set; }

    private void fillLowResoulutionMap()
    {
        var lowResTiles = new List<LowResolutionCell>();

        for (int startingRow = 0; startingRow < game.Board.Width; startingRow += TileSize)
        {
            for (int startingCol = 0; startingCol < game.Board.Height; startingCol += TileSize)
            {
                bool isInChunk(Location l) =>
                    l.Row >= startingRow && l.Row < (startingRow + TileSize) &&
                    l.Column >= startingCol && l.Column < (startingCol + TileSize);

                var innerCells = game.Board.Cells
                    .Where(c => isInChunk(c.Value.Location))
                    .Select(c => c.Value);
                var newTile = new LowResolutionCell(innerCells);
                lowResTiles.Add(newTile);
            }
        }

        LowResolution = lowResTiles.ToArray();
    }
}
