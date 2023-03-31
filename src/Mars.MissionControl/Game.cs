using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Mars.MissionControl;
public class Game : IDisposable
{
    public Game(GameCreationOptions startOptions, ILogger<Game> logger)
    {
        if (!(startOptions.MapWithTargets.Targets?.Any() ?? false))
        {
            throw new CannotStartGameWithoutTargetsException();
        }

        GameState = GameState.Joining;
        Board = new Board(startOptions.MapWithTargets.Map);
        Map = startOptions.MapWithTargets.Map;
        Targets = new ReadOnlyCollection<Location>(startOptions.MapWithTargets.Targets.ToList());
        PerseveranceVisibilityRadius = startOptions.PerseveranceVisibilityRadius;
        IngenuityVisibilityRadius = startOptions.IngenuityVisibilityRadius;
        StartingBatteryLevel = startOptions.StartingBatteryLevel;
        IngenuityStartingBatteryLevel = Board.Width * 2 + Board.Height * 2;
        this.logger = logger;
    }

    public int MapNumber => Board.MapNumber;

    public ReadOnlyCollection<Location> Targets { get; }
    public int PerseveranceVisibilityRadius { get; }
    public int IngenuityVisibilityRadius { get; }
    public int StartingBatteryLevel { get; }
    public int IngenuityStartingBatteryLevel { get; }

    private readonly ILogger<Game> logger;

    public Map Map { get; private set; }
    private ConcurrentDictionary<PlayerToken, Player> players = new();
    private ConcurrentDictionary<string, PlayerToken> playerTokenCache = new();
    public bool TryTranslateToken(string tokenString, out PlayerToken? token) => playerTokenCache.TryGetValue(tokenString, out token);
    private static Orientation getRandomOrientation() => (Orientation)Random.Shared.Next(0, 4);
    public GamePlayOptions? GamePlayOptions { get; private set; }
    public GameState GameState { get; set; }
    public Board Board { get; private set; }
    private Timer? rechargeTimer;
    public DateTime GameStartedOn { get; private set; }
    public ReadOnlyCollection<Player> Players =>
        new ReadOnlyCollection<Player>(players.Values.ToList());
    private ConcurrentQueue<Player> winners = new();
    private readonly ConcurrentBag<TargetVisitation> targetVisitations = new();

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
        logger.LogInformation("New player came into existence and started at location ({x}, {y}) ", startingLocation.X, startingLocation.Y);
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
            logger.LogError($"Player {player.Token.Value} couldn't be added");
            throw new PlayerAlreadyExistsException("Unable to add new player...that token already exists?!");
        }

        raiseStateChange();

        return new JoinResult(
            player.Token,
            player.PerseveranceLocation,
            player.Orientation,
            player.BatteryLevel,
            Targets,
            Board.GetNeighbors(player.PerseveranceLocation, PerseveranceVisibilityRadius),
            Map.LowResolution
        );
    }

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
        rechargeTimer = new Timer(rechargeTimer_Callback, null, 1_000, 1_000);
    }

    private void rechargeTimer_Callback(object? _)
    {
        foreach (var playerToken in players.Keys)
        {
            var origPlayer = players[playerToken];
            var newPlayer = origPlayer with { BatteryLevel = origPlayer.BatteryLevel + GamePlayOptions!.RechargePointsPerSecond };
            players.TryUpdate(playerToken, newPlayer, origPlayer);
        }

        raiseStateChange();
    }

    public IngenuityMoveResult MoveIngenuity(PlayerToken token, Location destination, int updatePlayerTryAgainCount = 0)
    {
        if (updatePlayerTryAgainCount > 9)
        {
            throw new UnableToUpdatePlayerException();
        }
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

        var deltaX = Math.Abs(destination.X - player.IngenuityLocation.X);
        var deltaY = Math.Abs(destination.Y - player.IngenuityLocation.Y);
        var movementCost = Math.Max(deltaX, deltaY);

        if (player.IngenuityBatteryLevel < movementCost)
        {
            message = GameMessages.IngenuityOutOfBattery;
        }
        else if (destination.X < 0 || destination.Y < 0 || destination.X > Board.Height || destination.Y > Board.Width)
        {
            message = GameMessages.MovedOutOfBounds;
        }
        else if (deltaX >= 3 || deltaY >= 3)
        {
            message = GameMessages.IngenuityTooFar;
        }
        else
        {
            player = player with
            {
                IngenuityLocation = destination,
                IngenuityBatteryLevel = player.IngenuityBatteryLevel - movementCost
            };

            if (!players.TryUpdate(token, player, unmodifiedPlayer))
            {
                // if we are here because perserverence and ingenuity are
                // moving at the same time we should be able to retry the move
                System.Console.WriteLine("Recursing MoveIngenuity to avoid UnableToUpdatePlayerException");
                return MoveIngenuity(token, destination, updatePlayerTryAgainCount + 1);
            }
            message = GameMessages.IngenuityMoveOK;
        }

        raiseStateChange();
        logger.LogInformation("player: {name} moved helicopter correctly and moved to location: {loc}", player.Name, player.IngenuityLocation);

        return new IngenuityMoveResult(
            player.IngenuityLocation,
            player.IngenuityBatteryLevel,
            Board.GetNeighbors(player.IngenuityLocation, IngenuityVisibilityRadius),
            message ?? throw new Exception("Game message not set?!")
        );
    }

    public MoveResult MovePerseverance(PlayerToken token, Direction direction, int updatePlayerTryAgainCount = 0)
    {
        if (updatePlayerTryAgainCount > 9)
        {
            throw new UnableToUpdatePlayerException();
        }
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
                var newBatteryLevel = player.BatteryLevel - Board[desiredLocation].Difficulty.Value;
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
            logger.LogError("Recursing MoveIngenuity for {playerName} to avoid UnableToUpdatePlayerException", player.Name);
            return MovePerseverance(token, direction, updatePlayerTryAgainCount + 1);
        }

        if (isOnATarget(player) && playerHasNotVisitedThisTarget(player))
        {
            recordTargetVisitation(player);
            message = GameMessages.YouMadeItToTheTarget;
            if (hasVisitedAllTargets(player))
            {
                logger.LogInformation("Player {playerName} has won!", player.Name);
                players.Remove(player.Token, out _);
                player = player with { WinningTime = DateTime.Now - GameStartedOn };
                winners.Enqueue(player);
                message = GameMessages.YouMadeItToAllTheTargets;
            }
        }

        raiseStateChange();
        logger.LogInformation("player: {playerName} moved rover correctly", player.Name);

        return new MoveResult(
            player.PerseveranceLocation,
            player.BatteryLevel,
            player.Orientation,
            Board.GetNeighbors(player.PerseveranceLocation, PerseveranceVisibilityRadius),
            message ?? throw new Exception("Game message not set?!")
        );
    }

    private bool playerHasNotVisitedThisTarget(Player player) =>
        !targetVisitations.Any(tv => tv.Player.Token == player.Token && tv.Target == player.PerseveranceLocation);

    private bool isOnATarget(Player player) => Targets.Contains(player.PerseveranceLocation);

    private void recordTargetVisitation(Player player) =>
        targetVisitations.Add(new TargetVisitation(player.PerseveranceLocation, player, DateTime.Now));

    private bool hasVisitedAllTargets(Player player)
    {
        var targetsVisited = targetVisitations.Where(tv => tv.Player.Token == player.Token).Select(tv => tv.Target);
        return !Targets.Except(targetsVisited).Any();
    }

    public Location GetPlayerLocation(PlayerToken token) => players[token].PerseveranceLocation;

    public void Dispose()
    {
        rechargeTimer?.Dispose();
    }
}
public record TargetVisitation(Location Target, Player Player, DateTime Timestamp);

public static class GameMessages
{
    public const string MovedOutOfBounds = "Looks like you tried to move beyond the borders of the game.";
    public const string TurnedOK = "Turned OK";
    public const string MovedOK = "Moved OK";
    public const string YouMadeItToTheTarget = "You made it to the target!";
    public const string YouMadeItToAllTheTargets = "You made it to all the targets!";
    public const string InsufficientBattery = "Insufficient battery to make move.  Wait and recharge your battery.";
    public const string IngenuityOutOfBattery = "Ingenuity does not have sufficient battery.";
    public const string IngenuityMoveOK = "Ingenuity moved OK.";
    public const string IngenuityTooFar = "Ingenuity cannot fly that far at once.";
}
