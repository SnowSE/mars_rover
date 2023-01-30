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
		Game = new Game(startOptions.Width, startOptions.Height);
	}
}

public class GameStartOptions
{
	public int Height { get; set; } = 100;
	public int Width { get; set; } = 100;
}