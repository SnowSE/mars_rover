using Mars.Web;
using System.Collections.Concurrent;

public class MultiGameHoster
{
    public ConcurrentDictionary<string, GameManager> Games { get; } = new();
    public ConcurrentDictionary<string, string> TokenMap { get; } = new();

    private string nextGame = "y";
    private object lockObject = new();

    public string MakeNewGame()
    {
        lock (lockObject)
        {
            var gameId = nextGame;
            Games.TryAdd(gameId, new GameManager());

            nextGame = incrementGameId(nextGame);
            return gameId;
        }
    }

    private string incrementGameId(string nextGame)
    {
        var chars = nextGame.ToCharArray();
        var lastIndex = chars.Length - 1;
        if (chars[lastIndex] < 'z')
        {
            chars[lastIndex]++;
            return new string(chars);
        }
        else
        {
            chars[lastIndex] = 'a';
            return new string(chars) + "a";
        }
    }
}