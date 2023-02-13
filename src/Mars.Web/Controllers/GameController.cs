namespace Mars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    ConcurrentDictionary<string, GameManager> games;
    private readonly ConcurrentDictionary<string, string> tokenMap;
	private readonly ILogger<GameController> logger;

	public GameController(MultiGameHoster multiGameHoster, ILogger<GameController> logger)
    {
        this.games = multiGameHoster.Games;
        this.tokenMap = multiGameHoster.TokenMap;
		this.logger = logger;
	}

    /// <summary>
    /// Join an existing game.  You can join in the 'Joining' state, or in the 'Playing' state.
    /// </summary>
    /// <param name="gameId">What game you'd like to join</param>
    /// <param name="name">What your player name should be</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<JoinResponse> Join(string gameId, string name)
    {
        if (games.TryGetValue(gameId, out GameManager? gameManager))
        {
            try
            {
                var joinResult = gameManager.Game.Join(name);
                tokenMap.TryAdd(joinResult.Token.Value, gameId);
				logger.LogInformation($"Player {name} joined game {gameId}");

				return new JoinResponse
                {
                    Token = joinResult.Token.Value,
                    StartingColumn = joinResult.PlayerLocation.Column,
                    StartingRow = joinResult.PlayerLocation.Row,
                    Neighbors = joinResult.Neighbors.ToDto(),
                    LowResolutionMap = joinResult.LowResolutionMap.ToDto(),
                    TargetRow = joinResult.TargetLocation.Row,
                    TargetColumn = joinResult.TargetLocation.Column,
                    Orientation = joinResult.Orientation.ToString()
                };
            }
            catch (TooManyPlayersException)
			{
				logger.LogError($"Player {name} failed to join game {gameId}. Too many players");
				return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
            }
        }
        else
		{
			logger.LogError($"Player {name} failed to join game {gameId}. Game id not found");
			return Problem("Unrecognized game id.", statusCode: 400, title: "Bad Game ID");
        }
    }

    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<StatusResponse> Status(string token)
    {
        if (tokenMap.TryGetValue(token, out string? gameId))
        {
            if (games.TryGetValue(gameId, out var gameManager))
            {
                if (gameManager.Game.TryTranslateToken(token, out _))
                {
                    return new StatusResponse { Status = gameManager.Game.GameState.ToString() };
                }
            }
        }

        return BadToken(token);
    }

    /// <summary>
    /// Move the Perseverance rover.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="direction">If left out, a default direction of Forward will be assumed.</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<MoveResponse> MovePerseverance(string token, Direction direction)
    {
        if (tokenMap.TryGetValue(token, out string? gameId))
        {
            if (games.TryGetValue(gameId, out var gameManager))
            {
                PlayerToken? playerToken;
                if (!gameManager.Game.TryTranslateToken(token, out playerToken))
                {
                    return BadToken(token);
                }

                if (gameManager.Game.GameState != GameState.Playing)
                {
                    return CantMove(ex.Message);
				}

                try
                {
                    var moveResult = gameManager.Game.MovePerseverance(playerToken!, direction);
                    return new MoveResponse
                    {
                        Row = moveResult.Location.Row,
                        Column = moveResult.Location.Column,
                        BatteryLevel = moveResult.BatteryLevel,
                        Neighbors = moveResult.Neighbors.ToDto(),
                        Message = moveResult.Message,
                        Orientation = moveResult.Orientation.ToString()
                    };
                }
                catch (Exception ex)
                {
                    return CantMove(ex.Message);
				}
            }
        }

        return BadToken(token);
	}

    /// <summary>
    /// Move the Ingenuity helicopter.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="destinationColumn"></param>
    /// <param name="destinationRow"></param>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IngenuityMoveResponse> MoveIngenuity(string token, int destinationRow, int destinationColumn)
    {
        if (tokenMap.TryGetValue(token, out string? gameId))
        {
            if (games.TryGetValue(gameId, out var gameManager))
            {
                PlayerToken? playerToken;
                if (!gameManager.Game.TryTranslateToken(token, out playerToken))
                {
                    return BadToken(token);
				}

                if (gameManager.Game.GameState != GameState.Playing)
                {
                    return CantMove("Game not in the Playing state");
				}

                try
                {
                    var moveResult = gameManager.Game.MoveIngenuity(playerToken!, new Location(destinationRow, destinationColumn));
                    return new IngenuityMoveResponse
                    {
                        Row = moveResult.Location.Row,
                        Column = moveResult.Location.Column,
                        Neighbors = moveResult.Neighbors.ToDto(),
                        Message = moveResult.Message,
                        BatteryLevel = moveResult.BatteryLevel
                    };
                }
                catch (Exception ex)
                {
                    return CantMove(ex.Message);
				}
            }
        }

        return BadToken(token);
	}

	public ObjectResult BadToken(string token)
	{
		logger.LogError($"Unrecogized token {token}");
		return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
	}

	public ObjectResult CantMove(string message)
	{
		logger.LogError($"Could not move: {message}");
		return Problem("Unable to move", statusCode: 400, title: message);
	}
}
