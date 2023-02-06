using System.Collections.ObjectModel;
using static Mars.MissionControl.Game;

namespace Mars.MissionControl;
public class Game : IDisposable
{
    public Game(GameStartOptions startOptions)
    {
        GameState = GameState.Joining;
        Board = new Board(startOptions.Map);
        Map = startOptions.Map;
        TargetLocation = new Location(Map.Width / 2, Map.Height / 2);
        PerseveranceVisibilityRadius = startOptions.PerseveranceVisibilityRadius;
        IngenuityVisibilityRadius = startOptions.IngenuityVisibilityRadius;
        StartingBatteryLevel = startOptions.StartingBatteryLevel;
        IngenuityStartingBatteryLevel = Board.Width * 2 + Board.Height * 2;
    }

    public int MapNumber => Board.MapNumber;

    public Location TargetLocation { get; private set; }
    public int PerseveranceVisibilityRadius { get; }
    public int IngenuityVisibilityRadius { get; }
    public int StartingBatteryLevel { get; }
    public int IngenuityStartingBatteryLevel { get; }
    public Map Map { get; private set; }
    private ConcurrentDictionary<PlayerToken, Player> players = new();
    private ConcurrentDictionary<string, PlayerToken> playerTokenCache = new();

    public ReadOnlyCollection<Player> Players =>
        new ReadOnlyCollection<Player>(players.Values.ToList());
    private ConcurrentQueue<Player> winners = new();
    public IEnumerable<Player> Winners => winners.ToArray();

    #region State Changed
    public event EventHandler? GameStateChanged;
    public DateTime lastStateChange;
    public TimeSpan stateChangeFrequency;// = TimeSpan.FromSeconds(1);
    private void raiseStateChange()
    {
        if (lastStateChange + stateChangeFrequency < DateTime.Now)
        {
            GameStateChanged?.Invoke(this, EventArgs.Empty);
            lastStateChange = DateTime.Now;
        }
    }
    #endregion

    public JoinResult Join(string playerName)
    {
        if (GameState != GameState.Joining && GameState != GameState.Playing)
        {
            throw new InvalidGameStateException();
        }

        var player = new Player(playerName);
        var startingLocation = Board.PlaceNewPlayer(player);
        player = player with
        {
            BatteryLevel = StartingBatteryLevel,
            PerseveranceLocation = startingLocation,
            IngenuityLocation = startingLocation,
            IngenuityBatteryLevel = StartingBatteryLevel,
            Orientation = getRandomOrientation()
        };
        if (!players.TryAdd(player.Token, player) ||
           !playerTokenCache.TryAdd(player.Token.Value, player.Token))
        {
            throw new Exception("Unable to add new player...that token already exists?!");
        }

        raiseStateChange();

        return new JoinResult(
            player.Token,
            player.PerseveranceLocation,
            player.Orientation,
            player.BatteryLevel,
            TargetLocation,
            Board.GetNeighbors(player.PerseveranceLocation, PerseveranceVisibilityRadius),
            Map.LowResolution
        );
    }

    private static Orientation getRandomOrientation()
    {
        return (Orientation)Random.Shared.Next(0, 4);
    }

    public GamePlayOptions? GamePlayOptions { get; private set; }
    public GameState GameState { get; set; }
    public Board Board { get; private set; }
    private Timer? rechargeTimer;
    public DateTime GameStartedOn { get; private set; }

    public void PlayGame() => PlayGame(new GamePlayOptions());

    public void PlayGame(GamePlayOptions gamePlayOptions)
    {
        if (GameState != GameState.Joining)
        {
            throw new InvalidGameStateException($"Cannot play game if currently {GameState}");
        }

        GamePlayOptions = gamePlayOptions;
        GameState = GameState.Playing;
        GameStartedOn = DateTime.Now;
        rechargeTimer = new Timer(timer_Callback, null, 1_000, 1_000);
    }

    private void timer_Callback(object? _)
    {
        foreach (var playerToken in players.Keys)
        {
            var origPlayer = players[playerToken];
            if (origPlayer.BatteryLevel < StartingBatteryLevel)
            {
                var newPlayer = origPlayer with { BatteryLevel = Math.Min(StartingBatteryLevel, origPlayer.BatteryLevel + GamePlayOptions!.RechargePointsPerSecond) };
                players.TryUpdate(playerToken, newPlayer, origPlayer);
            }
        }
        raiseStateChange();
    }

    public IngenuityMoveResult MoveIngenuity(PlayerToken token, Location destination)
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
        var unmodifiedPlayer = player;
        string? message;

        var deltaRow = Math.Abs(destination.Row - player.IngenuityLocation.Row);
        var deltaCol = Math.Abs(destination.Column - player.IngenuityLocation.Column);
        var movementCost = Math.Max(deltaCol, deltaCol);

        if (player.IngenuityBatteryLevel < movementCost)
        {
            message = GameMessages.IngenuityOutOfBattery;
        }
        else if (destination.Row < 0 || destination.Column < 0 || destination.Row > Board.Height || destination.Column > Board.Width)
        {
            message = GameMessages.MovedOutOfBounds;
        }
        else if(deltaRow >= 3 || deltaCol >= 3)
        {
            message = GameMessages.IngenuityTooFar;
        }
        else
        {
            player = player with 
            {
                IngenuityLocation = destination ,
                IngenuityBatteryLevel = player.IngenuityBatteryLevel - movementCost
            };
            message = GameMessages.IngenuityMoveOK;
        }

        if (!players.TryUpdate(token, player, unmodifiedPlayer))
        {
            throw new UnableToUpdatePlayerException();
        }

        raiseStateChange();

        return new IngenuityMoveResult(
            player.IngenuityLocation,
            player.IngenuityBatteryLevel,
            Board.GetNeighbors(player.PerseveranceLocation, IngenuityVisibilityRadius),
            message ?? throw new Exception("Game message not set?!")
        );
    }

    public MoveResult MovePerseverance(PlayerToken token, Direction direction)
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
        var unmodifiedPlayer = player;
        string? message;

        if (direction == Direction.Right || direction == Direction.Left)
        {
            player = player with
            {
                BatteryLevel = player.BatteryLevel - 1,
                Orientation = player.Orientation.Turn(direction)
            };
            message = GameMessages.TurnedOK;
        }
        else
        {
            var desiredLocation = direction switch
            {
                Direction.Forward => player.CellInFront(),
                Direction.Reverse => player.CellInBack(),
                _ => throw new Exception("What direction do you think you're going?")
            };

            if (Board.Cells.ContainsKey(desiredLocation) is false)
            {
                player = player with
                {
                    BatteryLevel = player.BatteryLevel - 1
                };
                message = GameMessages.MovedOutOfBounds;
            }
            else
            {
                int newBatteryLevel = player.BatteryLevel - Board[desiredLocation].Difficulty.Value;
                if (newBatteryLevel >= 0)
                {
                    player = player with
                    {
                        BatteryLevel = newBatteryLevel,
                        PerseveranceLocation = desiredLocation
                    };
                    message = GameMessages.MovedOK;
                }
                else
                {
                    message = GameMessages.InsufficientBattery;
                }
            }
        }

        if (!players.TryUpdate(token, player, unmodifiedPlayer))
        {
            throw new UnableToUpdatePlayerException();
        }

        if (player.PerseveranceLocation == TargetLocation)//you win!
        {
            players.Remove(player.Token, out _);
            player = player with { WinningTime = DateTime.Now - GameStartedOn };
            winners.Enqueue(player);
            message = GameMessages.YouMadeItToTheTarget;
        }

        raiseStateChange();

        return new MoveResult(
            player.PerseveranceLocation,
            player.BatteryLevel,
            player.Orientation,
            Board.GetNeighbors(player.PerseveranceLocation, PerseveranceVisibilityRadius),
            message ?? throw new Exception("Game message not set?!")
        );
    }

    public Location GetPlayerLocation(PlayerToken token) => players[token].PerseveranceLocation;
    public bool TryTranslateToken(string tokenString, out PlayerToken? token)
    {
        token = null;
        if (playerTokenCache.ContainsKey(tokenString))
        {
            token = playerTokenCache[tokenString];
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        rechargeTimer?.Dispose();
    }
}

public static class GameMessages
{
    public const string MovedOutOfBounds = "Looks like you tried to move beyond the borders of the game.";
    public const string TurnedOK = "Turned OK";
    public const string MovedOK = "Moved OK";
    public const string YouMadeItToTheTarget = "You made it to the target!";
    public const string InsufficientBattery = "Insufficient battery to make move.  Wait and recharge your battery.";
    public const string IngenuityOutOfBattery = "Ingenuity does not have sufficient battery.";
    public const string IngenuityMoveOK = "Ingenuity moved OK.";
    public const string IngenuityTooFar = "Ingenuity cannot fly that far at once.";
}

