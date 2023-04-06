namespace Mars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly IConfiguration config;
    private readonly ILogger<AdminController> logger;
    private readonly MultiGameHoster gameHoster;

    public AdminController(IConfiguration config, ILogger<AdminController> logger, MultiGameHoster gameHoster)
    {
        this.config = config;
        this.logger = logger;
        this.gameHoster = gameHoster;
    }

    [HttpPost("[action]")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public IActionResult StartGame(StartGameRequest request)
    {
        if (request.Password != config[ConfigKeys.GamePassword])
            return Problem("Invalid password", statusCode: 400, title: "Cannot start game with invalid password.");

        if (gameHoster.Games.TryGetValue(request.GameID, out var gameManager))
        {
            try
            {
                var gamePlayOptions = new GamePlayOptions
                {
                    RechargePointsPerSecond = request.RechargePointsPerSecond,
                };
                logger.LogInformation("Starting game play via admin api");
                gameManager.Game.PlayGame(gamePlayOptions);
                return Ok("Game started OK");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 400, title: "Error starting game");
            }
        }

        return Problem("Invalid GameID", statusCode: 400, title: "Invalid Game ID");
    }
}

public class StartGameRequest
{
    public int RechargePointsPerSecond { get; set; }
    public string? Password { get; set; }
    public string GameID { get; set; }
}