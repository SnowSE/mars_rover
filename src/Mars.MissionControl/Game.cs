using System.ComponentModel;

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
    }

    public int MapNumber => Board.MapNumber;

    public Location TargetLocation { get; private set; }
    public Map Map { get; private set; }
    private ConcurrentDictionary<PlayerToken, Player> players = new();
    private ConcurrentDictionary<string, PlayerToken> playerTokenCache = new();

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

    public PlayerToken Join(string playerName)
    {
        if (GameState != GameState.Joining)
        {
            throw new InvalidGameStateException();
        }

        var player = new Player(playerName);
        Board.PlaceNewPlayer(player);
        players.TryAdd(player.Token, player);
        playerTokenCache.TryAdd(player.Token.Value, player.Token);
        raiseStateChange();
        return player.Token;
    }

    public GameState GameState { get; set; }
    public Board Board { get; private set; }

    public void StartGame()
    {
        GameState = GameState.Playing;
    }

    public Location Move(PlayerToken token, Direction direction)
    {
        if (GameState != GameState.Playing)
        {
            throw new InvalidGameStateException();
        }

        if (players.ContainsKey(token) is false)
        {
            throw new UnrecognizedTokenException();
        }
        
        var player = players[token];
        var currentLocation = Board.RoverLocations[player];
        var newLocation = direction switch
        {
            Direction.Forward => new Location(currentLocation.Row + 1, currentLocation.Column),
            Direction.Left => new Location(currentLocation.Row, currentLocation.Column - 1),
            Direction.Right => new Location(currentLocation.Row, currentLocation.Column + 1),
            Direction.Reverse => new Location(currentLocation.Row - 1, currentLocation.Column),
            _ => throw new InvalidEnumArgumentException()
        };

        if (newLocation.Row < 0 || newLocation.Row >= Board.Width || newLocation.Column < 0 || newLocation.Column >= Board.Height)
        {
            throw new InvalidMoveException();
        }
            
        Board.RoverLocations[player] = newLocation;
        return newLocation;
    }

    public Location GetPlayerLocation(PlayerToken token)
    {
        return Board.RoverLocations
            .Single(kvp => kvp.Key.Token == token).Value;
    }

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

public class GameAlreadyStartedException : Exception
{
    public GameAlreadyStartedException()
    {
    }

    public GameAlreadyStartedException(string? message) : base(message)
    {
    }

    public GameAlreadyStartedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GameAlreadyStartedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}