using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Mars.MissionControl;
public partial class Game : IDisposable
{
	public Game(GameCreationOptions startOptions, ILogger<Game> logger)
	{
		if (!(startOptions?.MapWithTargets?.Targets?.Any() ?? false))
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
		IngenuitiesPerPlayer = startOptions.NumberOfIngenuitiesPerPlayer;
		IngenuityStartingBatteryLevel = Board.Width * 2 + Board.Height * 2;
		MinimumBatteryThreshold = startOptions.MinimumBatteryThreshold;
		KeepTheGameGoingBatteryBoostAmount = startOptions.KeepTheGameGoingBatteryBoostAmount;
		this.logger = logger;
	}

	public int MapNumber => Board.MapNumber;

	public ReadOnlyCollection<Location> Targets { get; }
	public int PerseveranceVisibilityRadius { get; }
	public int IngenuityVisibilityRadius { get; }
	public int StartingBatteryLevel { get; }
	public int IngenuityStartingBatteryLevel { get; }
	public int MinimumBatteryThreshold { get; }
	public int IngenuitiesPerPlayer { get; }
	public int KeepTheGameGoingBatteryBoostAmount { get; }

	private readonly ILogger<Game> logger;

	public Map Map { get; private set; }
	private readonly ConcurrentDictionary<PlayerToken, Player> players = new();
	private readonly ConcurrentDictionary<(PlayerToken, int), Ingenuity> ingenuities = new();
	private readonly ConcurrentDictionary<string, PlayerToken> playerTokenCache = new();
	public bool TryTranslateToken(string tokenString, out PlayerToken? token) => playerTokenCache.TryGetValue(tokenString, out token);
	private static Orientation getRandomOrientation() => (Orientation)Random.Shared.Next(0, 4);
	public GamePlayOptions? GamePlayOptions { get; private set; }
	public GameState GameState { get; set; }
	public Board Board { get; private set; }
	private Timer? rechargeTimer;
	public DateTime GameStartedOn { get; private set; }
	public ReadOnlyCollection<Player> Players => new(players.Values.ToList());
	public ReadOnlyCollection<Ingenuity> Ingenuities => new(ingenuities.Values.ToList());
	private readonly ConcurrentQueue<Player> winners = new();
	private readonly ConcurrentBag<TargetVisitation> targetVisitations = new();

	public IEnumerable<Player> Winners => winners.ToArray();

	[LoggerMessage(1, LogLevel.Information, "New player came into existence and started at location ({x}, {y}) ")] partial void LogNewPlayerJoined(int x, int y);
	[LoggerMessage(2, LogLevel.Error, "Player {token} couldn't be added")] partial void LogPlayerCouldNotBeAdded(string token);
	[LoggerMessage(3, LogLevel.Error, "Unable to update engenuities list, exceeded retry count.")] partial void LogUnableToUpdateIngenuitiesExceededRetry();
	[LoggerMessage(4, LogLevel.Warning, "Recursing MoveIngenuity to avoid UnableToUpdateIngenuityException.  Try {updatePlayerTryAgainCount}")] partial void LogRetryUpdateIngenuities(int updatePlayerTryAgainCount);
	[LoggerMessage(5, LogLevel.Information, "player: {name} moved ingenuity {id} correctly and moved to location: {loc}")] partial void LogIngenuityMoved(string name, int id, Location loc);
	[LoggerMessage(6, LogLevel.Information, "All players are below minimum battery threshold of {minimumBatteryThreshold}; giving bonus of {keepTheGameGoingBatteryBoostAmount} to everyone")] partial void LogBatteryBoost(int minimumBatteryThreshold, int keepTheGameGoingBatteryBoostAmount);
	[LoggerMessage(7, LogLevel.Warning, "Recursing MoveIngenuity for {playerName} to avoid UnableToUpdatePlayerException")] partial void LogRetryUpdatePlayers(string playerName);
	[LoggerMessage(8, LogLevel.Information, "Player {playerName} has won!")] partial void LogPlayerWon(string playerName);
	[LoggerMessage(9, LogLevel.Information, "player: {playerName} moved rover correctly")] partial void LogPerseveranceMoved(string playerName);
	[LoggerMessage(10, LogLevel.Information, "Player {playerName} made it to target {location}")] partial void LogPlayerReachedTarget(string playerName, Location location);
	[LoggerMessage(11, LogLevel.Information, "Player {playerName} already visited target {location}...")] partial void LogPlayerAlreadyVisitedTarget(string playerName, Location location);

	#region State Changed
	public event EventHandler? GameStateChanged;
	private DateTime lastStateChange;
	private readonly TimeSpan stateChangeFrequency = TimeSpan.FromSeconds(1);
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
		if (GameState is not GameState.Joining and not GameState.Playing)
		{
			throw new InvalidGameStateException();
		}

		var player = new Player(playerName);
		var startingLocation = Board.PlaceNewPlayer(player);
		LogNewPlayerJoined(startingLocation.X, startingLocation.Y);

		addIngenuitiesForPlayer(player, startingLocation);

		player = player with
		{
			BatteryLevel = StartingBatteryLevel,
			PerseveranceLocation = startingLocation,
			Orientation = getRandomOrientation()
		};

		if (!players.TryAdd(player.Token, player) || !playerTokenCache.TryAdd(player.Token.Value, player.Token))
		{
			LogPlayerCouldNotBeAdded(player.Token.Value);
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

	private void addIngenuitiesForPlayer(Player player, Location startingLocation)
	{
		for (int i = 0; i < IngenuitiesPerPlayer; i++)
		{
			ingenuities.TryAdd((player.Token, i), new Ingenuity(i, player.Name, startingLocation, IngenuityStartingBatteryLevel));
		}
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

	public IngenuityMoveResult MoveIngenuity(PlayerToken token, int id, Location destination, int updatePlayerTryAgainCount = 0)
	{
		if (updatePlayerTryAgainCount > 9)
		{
			LogUnableToUpdateIngenuitiesExceededRetry();
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
		if (id < 0 || id >= IngenuitiesPerPlayer)
		{
			throw new InvalidIngenuityIdException();
		}

		var ingenuity = ingenuities[(player.Token, id)];
		string? message;

		var deltaX = Math.Abs(destination.X - ingenuity.Location.X);
		var deltaY = Math.Abs(destination.Y - ingenuity.Location.Y);
		var movementCost = Math.Max(deltaX, deltaY);

		if (ingenuity.BatteryLevel < movementCost)
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
			var unmodifiedIngenuity = ingenuity;
			ingenuity = ingenuity with { Location = destination, BatteryLevel = ingenuity.BatteryLevel - movementCost };
			if (!ingenuities.TryUpdate((token, id), ingenuity, unmodifiedIngenuity))
			{
				// if we are here because perserverence and ingenuity are
				// moving at the same time we should be able to retry the move
				LogRetryUpdateIngenuities(updatePlayerTryAgainCount);
				return MoveIngenuity(token, id, destination, updatePlayerTryAgainCount + 1);
			}

			message = GameMessages.IngenuityMoveOK;
		}

		raiseStateChange();
		LogIngenuityMoved(player.Name, id, destination);

		return new IngenuityMoveResult(
			ingenuity.Location,
			ingenuity.BatteryLevel,
			Board.GetNeighbors(ingenuity.Location, IngenuityVisibilityRadius),
			message ?? throw new MissingGameMessageException("Game message not set?!")
		);
	}

	public MoveResult MovePerseverance(PlayerToken token, Direction direction, int updatePlayerTryAgainCount = 0)
	{
		if (updatePlayerTryAgainCount > 9)
		{
			throw new UnableToUpdatePlayerException();
		}

		if (players.ContainsKey(token) is false)
		{
			throw new UnrecognizedTokenException();
		}

		var player = players[token];
		var unmodifiedPlayer = player;
		string? message;

		if (direction is Direction.Right or Direction.Left)
		{
			player = player with
			{
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
				_ => throw new InvalidDirectionException("What direction do you think you're going?")
			};

			if (GameState != GameState.Playing && isNotOnBorder(desiredLocation.X) && isNotOnBorder(desiredLocation.Y))
			{
				throw new InvalidGameStateException();
			}

			if (Board.Cells.ContainsKey(desiredLocation) is false)
			{
				message = GameMessages.MovedOutOfBounds;
			}
			else
			{
				var newBatteryLevel = player.BatteryLevel - Board[desiredLocation].Difficulty.Value;
				if (newBatteryLevel <= 0 && players.All(p => p.Value.BatteryLevel < MinimumBatteryThreshold))
				{
					LogBatteryBoost(MinimumBatteryThreshold, KeepTheGameGoingBatteryBoostAmount);
					foreach (var p in players)
					{
						players.TryUpdate(p.Key, p.Value with { BatteryLevel = KeepTheGameGoingBatteryBoostAmount }, p.Value);
					}

					newBatteryLevel = KeepTheGameGoingBatteryBoostAmount;
				}

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
			LogRetryUpdatePlayers(player.Name);
			return MovePerseverance(token, direction, updatePlayerTryAgainCount + 1);
		}

		if (isOnATarget(player) && thisIsTheNextTarget(player))
		{
			recordTargetVisitation(player);
			message = GameMessages.YouMadeItToTheTarget;
			if (hasVisitedAllTargets(player))
			{
				LogPlayerWon(player.Name);
				players.Remove(player.Token, out _);
				player = player with { WinningTime = DateTime.Now - GameStartedOn };
				winners.Enqueue(player);
				message = GameMessages.YouMadeItToAllTheTargets;
			}
		}

		raiseStateChange();
		LogPerseveranceMoved(player.Name);

		return new MoveResult(
			player.PerseveranceLocation,
			player.BatteryLevel,
			player.Orientation,
			Board.GetNeighbors(player.PerseveranceLocation, PerseveranceVisibilityRadius),
			message ?? throw new MissingGameMessageException("Game message not set?!")
		);
	}

	private bool isNotOnBorder(int value) => value != 0 && value != Board.Width - 1;

	private bool thisIsTheNextTarget(Player player)
	{
		var targetQueue = new Queue<Location>(Targets);
		var playerTargetVisitations = targetVisitations.Where(tv => tv.Token == player.Token);
		var nextTarget = targetQueue.Dequeue();
		while (targetQueue.Any() && playerTargetVisitations.Any(tv => tv.Target == nextTarget))
		{
			LogPlayerAlreadyVisitedTarget(player.Name, nextTarget);
			nextTarget = targetQueue.Dequeue();
		}

		return (player.PerseveranceLocation == nextTarget);
	}

	private bool isOnATarget(Player player) => Targets.Contains(player.PerseveranceLocation);

	private void recordTargetVisitation(Player player)
	{
		targetVisitations.Add(new TargetVisitation(player.PerseveranceLocation, player.Token, DateTime.Now));
		LogPlayerReachedTarget(player.Name, player.PerseveranceLocation);
	}

	private bool hasVisitedAllTargets(Player player)
	{
		var targetsVisited = targetVisitations.Where(tv => tv.Token == player.Token).Select(tv => tv.Target);
		return !Targets.Except(targetsVisited).Any();
	}

	public Location GetPlayerLocation(PlayerToken token) => players[token].PerseveranceLocation;

	public void Dispose()
	{
		rechargeTimer?.Dispose();
	}
}
public record TargetVisitation(Location Target, PlayerToken Token, DateTime Timestamp);

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
