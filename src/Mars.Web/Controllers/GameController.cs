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
            var token = gameManager.Game.Join(name);
            var location = gameManager.Game.GetPlayerLocation(token);
            return new JoinResponse
            {
                Token = token.Value,
                StartingColumn = location.Column,
                StartingRow = location.Row,
                Neighbors = gameManager.Game.Board.GetNeighbors(location, 2).Select(c => new Types.Cell()
                {
                    Column = c.Location.Column,
                    Row = c.Location.Row,
                    Difficulty = c.Difficulty.Value
                }),
                LowResolutionMap = gameManager.Game.Map.LowResolution.Select(t => new LowResolutionMapTile
                {
                    AverageDifficulty = t.AverageDifficulty.Value,
                    LowerLeftRow = t.LowerLeftRow,
                    LowerLeftColumn = t.LowerLeftColumn,
                    UpperRightColumn = t.UpperRightColumn,
                    UpperRightRow = t.UpperRightRow
                }),
                TargetRow = gameManager.Game.TargetLocation.Row,
                TargetColumn = gameManager.Game.TargetLocation.Column
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
    
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Move(string token, string direction)
    {
        if (gameManager.Game.TryTranslateToken(token, out var player))
        {
            if (Enum.TryParse<Direction>(direction, true, out var dir))
            {
                try
                {
                    gameManager.Game.Move(player, dir);
                }
                catch (InvalidMoveException){
                    return Problem($"You cannot move in that direction.", statusCode: 400, title: "Bad Move");
                }
                catch(InvalidGameStateException)
                {
                    return Problem($"You cannot move until the game starts.", statusCode: 400, title: "Game Not Started");
                }
                return Ok();
            }
            return Problem($"You gave an invalid direction. Valid directions are: Forward, Reverse, Right, Left.", statusCode: 400, title: "Bad Direction");
        }
        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
    }
}
