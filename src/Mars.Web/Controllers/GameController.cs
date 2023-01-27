using Mars.MissionControl;
using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc;

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
                //Neighbors = game.Board.GetNeighbors(location, 2);
            };
        }
        catch (TooManyPlayersException)
        {
            return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
        }
    }
}
