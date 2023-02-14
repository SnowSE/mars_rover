namespace Mars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    ConcurrentDictionary<string, GameManager> games;
    private readonly ConcurrentDictionary<string, string> tokenMap;

    public GameController(MultiGameHoster multiGameHoster)
    {
        this.games = multiGameHoster.Games;
        this.tokenMap = multiGameHoster.TokenMap;
    }

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
                var joinResult = gameManager.Game.Join(name);
                tokenMap.TryAdd(joinResult.Token.Value, gameId);

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
                return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
            }
        }
        else
        {
            return Problem("Unrecognized game id.", statusCode: 400, title: "Bad Game ID");
        }
    }

    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatusResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<StatusResponse> Status(string token)
    {
        if (tokenMap.TryGetValue(token, out string? gameId) &&
            games.TryGetValue(gameId, out var gameManager) &&
            gameManager.Game.TryTranslateToken(token, out _))
        {
            return new StatusResponse { Status = gameManager.Game.GameState.ToString() };
        }

        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
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
        if (tokenMap.TryGetValue(token, out string? gameId) && games.TryGetValue(gameId, out var gameManager))
        {
            PlayerToken? playerToken;
            if (!gameManager.Game.TryTranslateToken(token, out playerToken))
            {
                return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
            }

            if (gameManager.Game.GameState != GameState.Playing)
            {
                return Problem("Unable to move, invalid game state.", statusCode: 400, title: "Game not in the Playing state.");
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
            catch (Exception ex)
            {
                return Problem("Unable to move", statusCode: 400, title: ex.Message);
            }
        }

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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IngenuityMoveResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IngenuityMoveResponse> MoveIngenuity(string token, int destinationRow, int destinationColumn)
    {
        if (tokenMap.TryGetValue(token, out string? gameId) && games.TryGetValue(gameId, out var gameManager))
        {
            PlayerToken? playerToken;
            if (!gameManager.Game.TryTranslateToken(token, out playerToken))
            {
                return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
            }

            if (gameManager.Game.GameState != GameState.Playing)
            {
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

        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
    }
}
