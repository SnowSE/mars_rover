using System.Collections.ObjectModel;

namespace Mars.MissionControl;
public class Game
{
    public Game(int boardWidth = 5, int boardHeight = 5) : this(new GameStartOptions
    {
        Height = boardHeight,
        Width = boardWidth,
        MapNumber = 1
    })
    {
    }

    public Game(GameStartOptions startOptions)
    {
        if (startOptions.Width < 3 || startOptions.Height < 3)
        {
            throw new BoardTooSmallException();
        }
        GameState = GameState.Joining;
        Board = new Board(startOptions.Width, startOptions.Height, startOptions.MapNumber);
        Map = new Map(this);
        TargetLocation = new Location(startOptions.Width / 2, startOptions.Height / 2);
        PerseveranceVisibilityRadius = startOptions.PerseveranceVisibilityRadius;
        IngenuityVisibilityRadius = startOptions.IngenuityVisibilityRadius;
        StartingBatteryLevel = startOptions.StartingBatteryLevel;
    }

    public int MapNumber => Board.MapNumber;

    public Location TargetLocation { get; private set; }
    public int PerseveranceVisibilityRadius { get; }
    public int IngenuityVisibilityRadius { get; }
    public int StartingBatteryLevel { get; }
    public Map Map { get; private set; }
    private ConcurrentDictionary<PlayerToken, Player> players = new();
    private ConcurrentDictionary<string, PlayerToken> playerTokenCache = new();

    public ReadOnlyCollection<Player> Players =>
        new ReadOnlyCollection<Player>(players.Values.ToList());

    #region State Changed
    public event EventHandler? GameStateChanged;
    public DateTime lastStateChange;
    public TimeSpan stateChangeFrequency;
    private void raiseStateChange()
    {
        if (lastStateChange + stateChangeFrequency < DateTime.Now)
        {
            GameStateChanged?.Invoke(this, EventArgs.Empty);
            lastStateChange = DateTime.Now;
        }
    }
    #endregion

    public JoinResult Join(string playerName)
    {
        if (GameState != GameState.Joining)
        {
            throw new InvalidGameStateException();
        }

        var player = new Player(playerName) { BatteryLevel = StartingBatteryLevel };
        player = player with
        {
            Location = Board.PlaceNewPlayer(player),
            Direction = getRandomDirection()
        };
        if (!players.TryAdd(player.Token, player) ||
           !playerTokenCache.TryAdd(player.Token.Value, player.Token))
        {
            throw new Exception("Unable to add new player...that token already exists?!");
        }

        raiseStateChange();

        return new JoinResult(
            player.Token,
            player.Location,
            player.Direction,
            player.BatteryLevel,
            TargetLocation,
            Board.GetNeighbors(player.Location, PerseveranceVisibilityRadius),
            Map.LowResolution
        );
    }

    private static Direction getRandomDirection()
    {
        return (Direction)Random.Shared.Next(0, 3);
    }

    public GamePlayOptions GamePlayOptions { get; private set; }
    public GameState GameState { get; set; }
    public Board Board { get; private set; }

    public void PlayGame() => PlayGame(new GamePlayOptions());

    public void PlayGame(GamePlayOptions gamePlayOptions)
    {
        GamePlayOptions = gamePlayOptions;
        GameState = GameState.Playing;
    }

    public MoveResult MovePerseverance(PlayerToken token, Direction direction)
    {
        if (GameState != GameState.Playing)
        {
            throw new InvalidGameStateException();
        }

        if (players.ContainsKey(token) is false)
        {
            throw new UnrecognizedTokenException();
        }

        //actually move the player...

        var player = players[token];
        return new MoveResult(
            player.Location,
            player.BatteryLevel,
            Board.GetNeighbors(player.Location, PerseveranceVisibilityRadius),
            "Move OK");
    }

    public Location GetPlayerLocation(PlayerToken token) => players[token].Location;
    public bool TryTranslateToken(string tokenString, out PlayerToken token)
    {
        token = null;
        if (playerTokenCache.ContainsKey(tokenString))
        {
            token = playerTokenCache[tokenString];
            return true;
        }
        return false;
    }
}

public record JoinResult(PlayerToken Token, Location PlayerLocation, Direction Direction, int BatteryLevel, Location TargetLocation, IEnumerable<Cell> Neighbors, IEnumerable<LowResolutionCell> LowResolutionMap);
public record MoveResult(Location Location, int BatteryLevel, IEnumerable<Cell> Neighbors, string Message);

public enum GameState
{
    Joining,
    Playing,
    GameOver
}

public enum Direction
{
    Forward,
    Left,
    Right,
    Reverse
}
