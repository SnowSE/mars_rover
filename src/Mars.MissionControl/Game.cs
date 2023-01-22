using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Mars.MissionControl;
public class Game
{
    public Game()
    {
        GameState = GameState.Joining;
    }

    private ConcurrentBag<PlayerToken> players = new();

    public PlayerToken Join(string playerName)
    {
        if (GameState != GameState.Joining)
            throw new InvalidGameStateException();

        var token = PlayerToken.Generate();
        players.Add(token);
        return token;
    }

    public GameState GameState { get; set; }

    public void StartGame()
    {
        GameState = GameState.Playing;
    }

    public void Move(PlayerToken token, Direction direction)
    {
        if(GameState != GameState.Playing)
        {
            throw new InvalidGameStateException();
        }

        if (players.Contains(token) is false)
        {
            throw new UnrecognizedTokenException();
        }
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