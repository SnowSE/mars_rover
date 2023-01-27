﻿namespace Mars.MissionControl;
public class Game
{
    public Game(int boardWidth = 5, int boardHeight = 5)
    {
        if (boardWidth < 3 || boardHeight < 3)
        {
            throw new BoardTooSmallException();
        }
        GameState = GameState.Joining;
        Board = new Board(boardWidth, boardHeight);
    }

    private ConcurrentDictionary<PlayerToken, Player> players = new();

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
        raiseStateChange();
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
    }

    public Location GetPlayerLocation(PlayerToken token)
    {
        return Board.RoverLocations
            .Single(kvp => kvp.Key.Token == token).Value;
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