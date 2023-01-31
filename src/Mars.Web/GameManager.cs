namespace Mars.Web;

public class GameManager
{
	public GameManager()
	{
		StartNewGame(new GameStartOptions()
		{
			Height = 100,
			Width = 100
		});
	}

	public Game Game { get; private set; }
	public event EventHandler? GameStateChanged;

	public void StartNewGame(GameStartOptions startOptions)
	{
		//unsubscribe from old event
		if (Game != null)
		{
			Game.GameStateChanged -= Game_GameStateChanged;
		}

		Game = new Game(startOptions);
		GameStateChanged?.Invoke(this, EventArgs.Empty);

		//subscribe to new event
		Game.GameStateChanged += Game_GameStateChanged;
	}

	private void Game_GameStateChanged(object? sender, EventArgs e)
	{
		GameStateChanged?.Invoke(this, e);
	}
}
