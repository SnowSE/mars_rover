using Mars.Web;
using System.Collections.Concurrent;
using System.Text.Json;

public class MultiGameHoster
{
    public MultiGameHoster(IWebHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;

        parseMaps();
    }

    private void parseMaps()
    {
        List<List<Mars.MissionControl.Cell>> maps = new();
        var terrainFiles = Directory.GetFiles(Path.Combine(hostEnvironment.WebRootPath, "images"), "terrain_*.json");
        foreach (var file in terrainFiles)
        {
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
            maps.Add(cells);
        }
        ParsedMaps = maps.ToArray();
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

    public IEnumerable<Mars.MissionControl.Cell>[] ParsedMaps { get; private set; }
}