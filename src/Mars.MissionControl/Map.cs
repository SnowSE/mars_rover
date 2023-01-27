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

        for (int startingCol = 0; startingCol < game.Board.Width; startingCol += TileSize)
        {
            for (int startingRow = 0; startingRow < game.Board.Height; startingRow += TileSize)
            {
                bool isInChunk(Location l) =>
                    l.Row >= startingRow && l.Row < startingRow + TileSize &&
                    l.Column >= startingCol && l.Column < startingCol + TileSize;

                lowResTiles.Add(new LowResolutionCell(game.Board.Cells
                    .Where(c => isInChunk(c.Value.Location))
                    .Select(c => c.Value)));
            }
        }

        LowResolution = lowResTiles.ToArray();
    }
}
