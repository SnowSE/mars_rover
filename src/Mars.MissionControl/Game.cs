using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Mars.MissionControl;
public class Game
{
    public Game(int boardWidth = 5, int boardHeight = 5)
    {
        GameState = GameState.Joining;
        Board = new Board(boardWidth, boardHeight);
    }

    private ConcurrentDictionary<PlayerToken, Player> players = new();

    public PlayerToken Join(string playerName)
    {
        if (GameState != GameState.Joining)
        {
            throw new InvalidGameStateException();
        }

        var player = new Player(playerName);
        players.TryAdd(player.Token, player);
        Board.PlaceNewPlayer(player);
        return player.Token;
    }

    public GameState GameState { get; set; }
    public Board Board { get; private set; }

    public void StartGame()
    {
        GameState = GameState.Playing;
    }

    public void Move(PlayerToken token, Direction direction)
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

        Board.MovePlayer(player, direction);
    }

    public Location GetPlayerLocation(PlayerToken token)
    {
        return Board.FindPlayer(token);
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