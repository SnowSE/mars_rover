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
                Neighbors = joinResult.Neighbors.Select(c => new Types.Cell()
                {
                    Column = c.Location.Column,
                    Row = c.Location.Row,
                    Difficulty = c.Difficulty.Value
                }),
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
    //[ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<StatusResponse> Status(string token)
    {
        if (gameManager.Game.TryTranslateToken(token, out _))
        {
            return new StatusResponse { Status = gameManager.Game.GameState.ToString() };
        }
        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
    }
}
