namespace Mars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly Game game;

    public GameController(Game game)
    {
        this.game = game;
    }

    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<JoinResponse> Join(string name)
    {
        try
        {
            var token = game.Join(name);
            var location = game.GetPlayerLocation(token);
            return new JoinResponse
            {
                Token = token.Value,
                StartingColumn = location.Column,
                StartingRow = location.Row,
                Neighbors = game.Board.GetNeighbors(location, 2).Select(c => new Types.Cell()
                {
                    Column = c.Location.Column,
                    Row = c.Location.Row,
                    Difficulty = c.Difficulty.Value
                }),
                LowResolutionMap = game.Map.LowResolution.Select(t => new LowResolutionMapTile
                {
                    AverageDifficulty = t.AverageDifficulty.Value,
                    LowerLeftRow = t.LowerLeftRow,
                    LowerLeftColumn = t.LowerLeftColumn,
                    UpperRightColumn = t.UpperRightColumn,
                    UpperRightRow = t.UpperRightRow
                }),
                TargetRow = game.TargetLocation.Row,
                TargetColumn = game.TargetLocation.Column
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
        if (game.TryTranslateToken(token, out _))
        {
            return new StatusResponse { Status = game.GameState.ToString() };
        }
        return Problem("Unrecognized token", statusCode: 400, title: "Bad Token");
    }

    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Move(string token, string direction)
    {
        if (game.TryTranslateToken(token, out var player))
        {
            if (Enum.TryParse<Direction>(direction, true, out var dir))
            {
                try
                {
                    game.Move(player, dir);
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
