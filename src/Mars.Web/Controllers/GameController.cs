using Mars.MissionControl;
using Mars.Web.Pages;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<JoinResponse> Join(string gameId, string name)
    {
        using (logger.BeginScope($"GameName: {name}, GameId: {gameId}"))
        {
            if (games.TryGetValue(gameId, out GameManager? gameManager))
            {
                try
                {
                    var joinResult = gameManager.Game.Join(name);
                    tokenMap.TryAdd(joinResult.Token.Value, gameId);
                    using (logger.BeginScope($"UserToken: {joinResult.Token.Value}"))
                    {
                        logger.LogInformation($"New User {joinResult.Token.Value} Joined");
                    }

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
                    logger.LogError($"Too Many Players");
                    return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
                }
            }
            else
            {
                logger.LogError("Bad Game ID");
                return Problem("Unrecognized game id.", statusCode: 400, title: "Bad Game ID");
            }
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
            if (games.TryGetValue(gameId, out var gameManager))
            {
                if (gameManager.Game.TryTranslateToken(token, out _))
                {
                    return new StatusResponse { Status = gameManager.Game.GameState.ToString() };
                }
            }
        }
        logger.LogError("Unrecognized token");
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
    public ActionResult<MoveResponse> MovePerseverance(string token, Direction direction)
    {
        using (logger.BeginScope($"UserToken: {token}"))
        {
            if (tokenMap.TryGetValue(token, out string? gameId))
            {
                if (games.TryGetValue(gameId, out var gameManager))
                {
                    PlayerToken? playerToken;
                    if (!gameManager.Game.TryTranslateToken(token, out playerToken))
                    {
                        logger.LogError("Unrecognized token");
                        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
                    }

                    if (gameManager.Game.GameState != GameState.Playing)
                    {
                        logger.LogWarning("Unable to move Perserverance, invalid game state.");
                        return Problem("Unable to move, invalid game state.", statusCode: 400, title: "Game not in the Playing state.");
                    }

                    try
                    {
                        var moveResult = gameManager.Game.MovePerseverance(playerToken!, direction);
                        logger.LogInformation($"User {token} moved Perseverance {direction}. Move Result: {moveResult.Message}");
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
                        logger.LogWarning("Unable to move Perserverance");
                        return Problem("Unable to move", statusCode: 400, title: ex.Message);
                    }
                }
            }
            logger.LogError("Unrecognized token on move request");
            return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
        }
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
        using (logger.BeginScope($"UserToken: {token}"))
        {
            if (tokenMap.TryGetValue(token, out string? gameId))
            {
                if (games.TryGetValue(gameId, out var gameManager))
                {
                    PlayerToken? playerToken;
                    if (!gameManager.Game.TryTranslateToken(token, out playerToken))
                    {
                        logger.LogError("Unrecognized token on move request");
                        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
                    }

                    if (gameManager.Game.GameState != GameState.Playing)
                    {
                        logger.LogWarning("Unable to move Ingenuity");
                        return Problem("Unable to move, invalid game state.", statusCode: 400, title: "Game not in the Playing state.");
                    }

                    try
                    {
                        var moveResult = gameManager.Game.MoveIngenuity(playerToken!, new Location(destinationRow, destinationColumn));
                        logger.LogInformation($"User {token} moved Ingenuity");
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
                        logger.LogWarning("Unable to move Ingenuity");
                        return Problem("Unable to move", statusCode: 400, title: ex.Message);
                    }
                }
            }
            logger.LogError("Unrecognized token on move request");
            return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
        }
    }
}
