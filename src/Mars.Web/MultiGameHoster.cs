using Mars.Web;

public class MultiGameHoster
{
    public MultiGameHoster(IMapProvider mapProvider, ILoggerFactory logFactory, GameConfig gameConfig)
    {
        ParsedMaps = new List<Map>(mapProvider.LoadMaps());
        this.logger = logFactory.CreateLogger<MultiGameHoster>();
        this.gameLogger = logFactory.CreateLogger<Game>();
        this.gameConfig = gameConfig;
    }

    public void RaiseOldGamesPurged() => OldGamesPurged?.Invoke(this, EventArgs.Empty);

    public event EventHandler OldGamesPurged;
    public ConcurrentDictionary<string, GameManager> Games { get; } = new();
    public ConcurrentDictionary<string, string> TokenMap { get; } = new();

    private string nextGame = "a";
    private readonly object lockObject = new();
    private readonly ILogger<MultiGameHoster> logger;
    private readonly ILogger<Game> gameLogger;
    private readonly GameConfig gameConfig;

    public string MakeNewGame()
    {
        lock (lockObject)
        {
            var gameId = nextGame;
            Games.TryAdd(gameId, new GameManager(ParsedMaps, gameLogger, gameConfig));

            nextGame = IncrementGameId(nextGame);
            logger.LogInformation($"New Game Created: {gameId}");
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
            LowerLeftColumn = lowRes.LowerLeftY,
            LowerLeftRow = lowRes.LowerLeftX,
            UpperRightColumn = lowRes.UpperRightY,
            UpperRightRow = lowRes.UpperRightX
        };
    }
}
