using Mars.Web;
using System.Collections.Concurrent;

public class MultiGameHoster
{
    public void RaiseOldGamesPurged() => OldGamesPurged?.Invoke(this, EventArgs.Empty);

    public event EventHandler OldGamesPurged;
    public ConcurrentDictionary<string, GameManager> Games { get; } = new();
    public ConcurrentDictionary<string, string> TokenMap { get; } = new();

    private string nextGame = "a";
    private object lockObject = new();

    public string MakeNewGame()
    {
        lock (lockObject)
        {
            var gameId = nextGame;
            Games.TryAdd(gameId, new GameManager());

            nextGame = IncrementGameId(nextGame);
            return gameId;
        }
    }

    public static string IncrementGameId(string nextGame)
    {
        var chars = nextGame.ToCharArray();

        if (chars.All(c => c == 'z'))
        {
            return new string('a', chars.Length + 1);
        }

        var lastIndex = chars.Length - 1;
        if (chars[lastIndex] < 'z')
        {
            chars[lastIndex]++;
            return new string(chars);
        }

        chars[lastIndex--] = 'a';
        while (lastIndex >= 0)
        {
            if (chars[lastIndex] < 'z')
            {
                chars[lastIndex]++;
                break;
            }
            else
            {
                chars[lastIndex--] = 'a';
            }
        }

        return new string(chars);
    }
}