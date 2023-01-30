namespace Mars.Web;

public class GameManager
{
	public GameManager()
	{
		Game = new Game();
	}

	public Game Game { get; private set; }

	public void StartNewGame(GameStartOptions startOptions)
	{
		Game = new Game(startOptions.Width, startOptions.Heigh);
	}
}

public class GameStartOptions
{
	public int Heigh { get; set; } = 100;
	public int Width { get; set; } = 100;
}