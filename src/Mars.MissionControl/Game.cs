namespace Mars.MissionControl;
public class Game
{
    public Game()
    {
        GameStatus = new JoiningGameStatus();
    }

    public string Join(string playerName)
    {
        return IdGenerator.GetNextId();
    }

    public GameStatus GameStatus { get; set; }
}

public abstract class GameStatus
{
    public abstract string GameState { get; }
}

public class JoiningGameStatus : GameStatus
{
    public override string GameState => "Joining";
}
