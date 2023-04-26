using System.Diagnostics;

namespace Mars.Web.Controllers;

public static class GameActivitySource
{
	public static ActivitySource Instance { get; } = new ActivitySource("Mars.Web", "1.0");
}

[ApiController]
[Route("[controller]")]
public partial class GameController : ControllerBase
{
	readonly ConcurrentDictionary<string, GameManager> games;
	private readonly ConcurrentDictionary<string, string> tokenMap;
	private readonly ILogger<GameController> logger;

	public GameController(MultiGameHoster multiGameHoster, ILogger<GameController> logger)
	{
		this.games = multiGameHoster.Games;
		this.tokenMap = multiGameHoster.TokenMap;
		this.logger = logger;
	}

	[LoggerMessage(1, LogLevel.Warning, "Player {name} joined game {gameId}")] partial void LogPlayerJoinedGame(string name, string gameId);
	[LoggerMessage(2, LogLevel.Error, "Unrecogized token {token}")] partial void LogUnrecognizedToken(string token);
	[LoggerMessage(3, LogLevel.Warning, "Player {name} failed to join game {gameId}. Game id not found")] partial void LogGameIdNotFound(string name, string gameId);
	[LoggerMessage(4, LogLevel.Error, "Could not move into map area: Game not in Playing state.")] partial void LogGameNotInPlayingState();
	[LoggerMessage(5, LogLevel.Warning, "Could not move: {exceptionMessage}")] partial void LogUnableToMove(string exceptionMessage);
	[LoggerMessage(6, LogLevel.Warning, "Player {name} failed to join game {gameId}. Too many players")] partial void LogTooManyPlayers(string name, string gameId);

	/// <summary>
	/// Join an existing game.  You can join in the 'Joining' state, or in the 'Playing' state.
	/// </summary>
	/// <param name="gameId">What game you'd like to join</param>
	/// <param name="name">What your player name should be</param>
	/// <returns></returns>
	[HttpGet("[action]")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinResponse))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public ActionResult<JoinResponse> Join(string gameId, string name)
	{
		if (games.TryGetValue(gameId, out GameManager? gameManager))
		{
			try
			{
				using var activity = GameActivitySource.Instance.StartActivity("Join Game");
				var joinResult = gameManager.Game.Join(name);
				using (logger.BeginScope("ScopeUserToken: {ScopeUser} GameId: {ScopeGameId} ", joinResult.Token.Value, gameId))
				{
					tokenMap.TryAdd(joinResult.Token.Value, gameId);
					LogPlayerJoinedGame(name, gameId);
				}

				return new JoinResponse
				{
					Token = joinResult.Token.Value,
					StartingY = joinResult.PlayerLocation.Y,
					StartingX = joinResult.PlayerLocation.X,
					Neighbors = joinResult.Neighbors.ToDto(),
					LowResolutionMap = joinResult.LowResolutionMap.ToDto(),
					Targets = joinResult.TargetLocations.ToDto(),
					Orientation = joinResult.Orientation.ToString(),
					BatteryLevel = joinResult.BatteryLevel
				};
			}
			catch (TooManyPlayersException)
			{
				LogTooManyPlayers(name, gameId);
				return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
			}
		}
		else
		{
			LogGameIdNotFound(name, gameId);
			return Problem("Unrecognized game id.", statusCode: 400, title: "Bad Game ID");
		}
	}

	[HttpGet("[action]")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatusResponse))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public ActionResult<StatusResponse> Status(string token)
	{
		var tokenHasGame = tokenMap.TryGetValue(token, out string? gameId);
		using (logger.BeginScope("ScopeUserToken: {ScopeUser} GameId: {ScopeGameId} ", token, gameId))
		{
			if (tokenHasGame)
			{
				if (games.TryGetValue(gameId!, out var gameManager))
				{
					if (gameManager.Game.TryTranslateToken(token, out _))
					{
						return new StatusResponse { Status = gameManager.Game.GameState.ToString() };
					}
				}
			}

			LogUnrecognizedToken(token);
			return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
		}
	}

	/// <summary>
	/// Move the Perseverance rover.
	/// </summary>
	/// <param name="token"></param>
	/// <param name="direction">If left out, a default direction of Forward will be assumed.</param>
	/// <returns></returns>
	[HttpGet("[action]")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PerseveranceMoveResponse))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public ActionResult<PerseveranceMoveResponse> MovePerseverance(string token, Direction direction)
	{
		var tokenHasGame = tokenMap.TryGetValue(token, out string? gameId);

		using (logger.BeginScope("ScopeUserToken: {ScopeUser} GameId: {ScopeGameId} ", token, gameId))
		{
			if (tokenHasGame)
			{
				if (games.TryGetValue(gameId!, out var gameManager))
				{
					if (!gameManager.Game.TryTranslateToken(token, out var playerToken))
					{
						LogUnrecognizedToken(token);
						return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
					}

					try
					{
						var moveResult = gameManager.Game.MovePerseverance(playerToken!, direction);
						return new PerseveranceMoveResponse
						{
							X = moveResult.Location.X,
							Y = moveResult.Location.Y,
							BatteryLevel = moveResult.BatteryLevel,
							Neighbors = moveResult.Neighbors.ToDto(),
							Message = moveResult.Message,
							Orientation = moveResult.Orientation.ToString()
						};
					}
					catch (InvalidGameStateException)
					{
						LogGameNotInPlayingState();
						return Problem("Unable to move into map area", statusCode: 400, title: "Game not in Playing state.");
					}
					catch (Exception ex)
					{
						LogUnableToMove(ex.Message);
						return Problem("Unable to move", statusCode: 400, title: ex.Message);
					}
				}
			}

			LogUnrecognizedToken(token);
			return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
		}
	}

	/// <summary>
	/// Move the Ingenuity helicopter.
	/// </summary>
	/// <param name="token"></param>
	/// <param name="id">Which ingenuity helicopter you're moving</param>
	/// <param name="destinationY"></param>
	/// <param name="destinationX"></param>
	/// <returns></returns>
	[HttpGet("[action]")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IngenuityMoveResponse))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public ActionResult<IngenuityMoveResponse> MoveIngenuity(string token, int id, int destinationX, int destinationY)
	{
		var tokenHasGame = tokenMap.TryGetValue(token, out string? gameId);
		using (logger.BeginScope("ScopeUserToken: {ScopeUser} GameId: {ScopeGameId} ", token, gameId))
		{
			if (tokenHasGame && gameId is not null && games.TryGetValue(gameId, out var gameManager))
			{
				if (!gameManager.Game.TryTranslateToken(token, out var playerToken))
				{
					LogUnrecognizedToken(token);
					return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
				}

				if (gameManager.Game.GameState != GameState.Playing)
				{
					LogGameNotInPlayingState();
					return Problem("Unable to move", statusCode: 400, title: "Game not in Playing state.");
				}

				try
				{
					var moveResult = gameManager.Game.MoveIngenuity(playerToken!, id, new MissionControl.Location(destinationX, destinationY));
					return new IngenuityMoveResponse
					{
						X = moveResult.Location.X,
						Y = moveResult.Location.Y,
						Neighbors = moveResult.Neighbors.ToDto(),
						Message = moveResult.Message,
						BatteryLevel = moveResult.BatteryLevel
					};
				}
				catch (Exception ex)
				{
					LogUnableToMove(ex.Message);
					return Problem("Unable to move", statusCode: 400, title: ex.Message);
				}
			}

			LogUnrecognizedToken(token);
			return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
		}
	}
}
