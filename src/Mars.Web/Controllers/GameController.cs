namespace Mars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly GameManager gameManager;

    public GameController(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<JoinResponse> Join(string name)
    {
        try
        {
            var joinResult = gameManager.Game.Join(name);
            return new JoinResponse
            {
                Token = joinResult.Token.Value,
                StartingColumn = joinResult.PlayerLocation.Column,
                StartingRow = joinResult.PlayerLocation.Row,
                Neighbors = joinResult.Neighbors.ToDto(),
                LowResolutionMap = joinResult.LowResolutionMap.Select(t => new LowResolutionMapTile
                {
                    AverageDifficulty = t.AverageDifficulty.Value,
                    LowerLeftRow = t.LowerLeftRow,
                    LowerLeftColumn = t.LowerLeftColumn,
                    UpperRightColumn = t.UpperRightColumn,
                    UpperRightRow = t.UpperRightRow
                }),
                TargetRow = joinResult.TargetLocation.Row,
                TargetColumn = joinResult.TargetLocation.Column
            };
        }
        catch (TooManyPlayersException)
        {
            return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
        }
    }

    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<StatusResponse> Status(string token)
    {
        if (gameManager.Game.TryTranslateToken(token, out _))
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<MoveResponse> MovePerseverance(string token, Direction direction)
    {
        PlayerToken playerToken;
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
            var moveResult = gameManager.Game.MovePerseverance(playerToken, direction);
            return new MoveResponse
            {
                Row = moveResult.Location.Row,
                Column = moveResult.Location.Column,
                BatteryLevel = moveResult.BatteryLevel,
                Neighbors = moveResult.Neighbors.ToDto(),
                Message = moveResult.Message,
            };
        }
        catch (Exception ex)
        {
            return Problem("Unable to move", statusCode: 400, title: ex.Message);
        }
    }
}
