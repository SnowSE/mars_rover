using Mars.Web;
using System.Collections.Concurrent;
using System.Text.Json;

public class MultiGameHoster
{
    public MultiGameHoster(IWebHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;
        imagesFolder = Path.Combine(hostEnvironment.WebRootPath, "images");

        parseMaps();
    }

    string imagesFolder;

    private void parseMaps()
    {
        var terrainFiles = Directory.GetFiles(imagesFolder, "terrain_*.json");
        foreach (var file in terrainFiles)
        {
            var parts = Path.GetFileName(file).Split('_', '.');
            var mapNumber = int.Parse(parts[1]);
            var content = File.ReadAllText(file);
            var json = JsonSerializer.Deserialize<IEnumerable<IEnumerable<int>>>(content).ToList();
            var cells = new List<Mars.MissionControl.Cell>();
            for (int row = 0; row < json.Count; row++)
            {
                var cellsInRow = json[row].ToList();
                for (int col = 0; col < cellsInRow.Count; col++)
                {
                    cells.Add(new Mars.MissionControl.Cell(new Location(row, col), new Difficulty(cellsInRow[col])));
                }
            }

            var lowResCachedPath = Path.Combine(imagesFolder, Path.ChangeExtension(file, ".lowres.json"));
            var lowRes = (File.Exists(lowResCachedPath)) ?
                parseLowResolutionMap(lowResCachedPath) :
                fillLowResoulutionMap(cells, lowResCachedPath);
            ParsedMaps.Add(new Map(mapNumber, cells, lowRes));
        }
    }

    private List<LowResolutionCell> parseLowResolutionMap(string lowResCachedPath)
    {
        var content = File.ReadAllText(lowResCachedPath);
        var serializedTiles = JsonSerializer.Deserialize<IEnumerable<SerializedLowResolutionCell>>(content);
        return new(serializedTiles.Select(t => new LowResolutionCell(t.AverageDifficulty, t.LowerLeftRow, t.LowerLeftColumn, t.UpperRightRow, t.UpperRightColumn)));
    }

    private List<LowResolutionCell> fillLowResoulutionMap(List<Mars.MissionControl.Cell> cells, string lowResCachedPath)
    {
        var lowResTiles = Helpers.BuildLowResMap(cells);
        var serializableTiles = lowResTiles.Select(t => SerializedLowResolutionCell.FromLowResCel(t));
        File.WriteAllText(lowResCachedPath, JsonSerializer.Serialize(serializableTiles));
        return lowResTiles;
    }

    public void RaiseOldGamesPurged() => OldGamesPurged?.Invoke(this, EventArgs.Empty);

    public event EventHandler OldGamesPurged;
    public ConcurrentDictionary<string, GameManager> Games { get; } = new();
    public ConcurrentDictionary<string, string> TokenMap { get; } = new();

    private string nextGame = "a";
    private object lockObject = new();
    private readonly IWebHostEnvironment hostEnvironment;

    public string MakeNewGame()
    {
        lock (lockObject)
        {
            var gameId = nextGame;
            Games.TryAdd(gameId, new GameManager(ParsedMaps));

            nextGame = IncrementGameId(nextGame);
            return gameId;
        }
    }

    public static string IncrementGameId(string nextGame)
    {
        var chars = nextGame.ToCharArray();

        if (chars.All(c => c == 'z'))
        {
            return new string('a', chars.Length + 1);
        }

        var lastIndex = chars.Length - 1;
        if (chars[lastIndex] < 'z')
        {
            chars[lastIndex]++;
            return new string(chars);
        }

        chars[lastIndex--] = 'a';
        while (lastIndex >= 0)
        {
            if (chars[lastIndex] < 'z')
            {
                chars[lastIndex]++;
                break;
            }
            else
            {
                chars[lastIndex--] = 'a';
            }
        }

        return new string(chars);
    }

    public List<Map> ParsedMaps { get; private set; } = new();
}

public class SerializedLowResolutionCell
{
    public int AverageDifficulty { get; set; }
    public int LowerLeftRow { get; set; }
    public int LowerLeftColumn { get; set; }
    public int UpperRightRow { get; set; }
    public int UpperRightColumn { get; set; }

    public static SerializedLowResolutionCell FromLowResCel(LowResolutionCell lowRes)
    {
        return new SerializedLowResolutionCell
        {
            AverageDifficulty = lowRes.AverageDifficulty.Value,
            LowerLeftColumn = lowRes.LowerLeftColumn,
            LowerLeftRow = lowRes.LowerLeftRow,
            UpperRightColumn = lowRes.UpperRightColumn,
            UpperRightRow = lowRes.UpperRightRow
        };
    }
}
