namespace Mars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    ILogger<GameController> logger;
    ConcurrentDictionary<string, GameManager> games;
    private readonly ConcurrentDictionary<string, string> tokenMap;

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
                logger.LogInformation($"User joined game {gameId} with name {name} and token {joinResult.Token} at {DateTime.Now}");
                return new JoinResponse
                {
                    Token = joinResult.Token.Value,
                    StartingY = joinResult.PlayerLocation.Y,
                    StartingX = joinResult.PlayerLocation.X,
                    Neighbors = joinResult.Neighbors.ToDto(),
                    LowResolutionMap = joinResult.LowResolutionMap.ToDto(),
                    TargetX = joinResult.TargetLocation.X,
                    TargetY = joinResult.TargetLocation.Y,
                    Orientation = joinResult.Orientation.ToString()
                };
            }
            catch (TooManyPlayersException)
            {
                logger.LogError($"User {name} was prevented from joining game {gameId} due to too many players");
                return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
            }
        }
        else
        {
            logger.LogWarning($"User {name} tried to join game {gameId}, which does not exist");
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
                    logger.LogInformation($"User {token} requested status");
                    return new StatusResponse { Status = gameManager.Game.GameState.ToString() };
                }
            }
        }
        logger.LogWarning($"User requested status using invalid token {token}");
        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
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
                    logger.LogWarning($"User tried moving rover using invalid token {token}");
                    return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
                }

                if (gameManager.Game.GameState != GameState.Playing)
                {
                    logger.LogInformation($"User {token} tried moving rover before game started");
                    return Problem("Unable to move, invalid game state.", statusCode: 400, title: "Game not in the Playing state.");
                }

                try
                {

                    var moveResult = gameManager.Game.MovePerseverance(playerToken!, direction);
                    if(moveResult.Message == "You made it to the target!")
                    {
                        logger.LogInformation($"User {token} won at {DateTime.Now}");
                    }
                    return new MoveResponse
                    {
                        X = moveResult.Location.X,
                        Y = moveResult.Location.Y,
                        BatteryLevel = moveResult.BatteryLevel,
                        Neighbors = moveResult.Neighbors.ToDto(),
                        Message = moveResult.Message,
                        Orientation = moveResult.Orientation.ToString()
                    };
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"User {token} tried to move, but failed");
                    return Problem("Unable to move", statusCode: 400, title: ex.Message);
                }
            }
        }
        logger.LogWarning($"User tried moving rover using invalid token {token}");
        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
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
                    logger.LogWarning($"User tried moving drone using invalid token {token}");
                    return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
                }

                if (gameManager.Game.GameState != GameState.Playing)
                {
                    logger.LogInformation($"User {token} tried moving drone when the game wasn't ready");
                    return Problem("Unable to move, invalid game state.", statusCode: 400, title: "Game not in the Playing state.");
                }

                try
                {
                    var moveResult = gameManager.Game.MoveIngenuity(playerToken!, new Location(destinationRow, destinationColumn));
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
                    return Problem("Unable to move", statusCode: 400, title: ex.Message);
                }
            }
        }
        logger.LogWarning($"User tried moving drone using invalid token {token}");
        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
    }
}
