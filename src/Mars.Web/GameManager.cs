using System.Diagnostics.CodeAnalysis;

namespace Mars.Web;

public class GameManager
{
	public GameManager()
	{
		CreatedOn = DateTime.Now;

		StartNewGame(new GameStartOptions()
		{
			Height = 100,
			Width = 100
		});
	}

	public GameStartOptions GameStartOptions { get; } = new();
	public DateTime CreatedOn { get; }
	public Game Game { get; private set; }
	public event EventHandler? GameStateChanged;

	[MemberNotNull(nameof(Game))]
	public void StartNewGame(GameStartOptions startOptions)
	{
		//unsubscribe from old event
		if (Game != null)
		{
			Game.GameStateChanged -= Game_GameStateChanged;
			Game.Dispose();
		}

		Game = new Game(startOptions);
		GameStateChanged?.Invoke(this, EventArgs.Empty);

		//subscribe to new event
		Game.GameStateChanged += Game_GameStateChanged;
	}

	public void PlayGame(GamePlayOptions playOptions)
	{
		Game?.PlayGame(playOptions);
	}

	private void Game_GameStateChanged(object? sender, EventArgs e)
	{
		GameStateChanged?.Invoke(this, e);
	}
}
