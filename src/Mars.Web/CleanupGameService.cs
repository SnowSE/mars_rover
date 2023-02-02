namespace Mars.Web;

public class CleanupGameService : BackgroundService
{
    private readonly MultiGameHoster multiGameHoster;
    private readonly ILogger<CleanupGameService> logger;
    public readonly int CleanupFrequencyMinutes;

    public CleanupGameService(MultiGameHoster multiGameHoster, ILogger<CleanupGameService> logger, IConfiguration config)
    {
        this.multiGameHoster = multiGameHoster;
        this.logger = logger;
        if (!int.TryParse(config["CleanupFrequencyMinutes"], out CleanupFrequencyMinutes))
            CleanupFrequencyMinutes = 20;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(CleanupFrequencyMinutes), stoppingToken);

            logger.LogInformation("Looking for any old games to clean up:");
            var oldGames = multiGameHoster.Games.Where(g => g.Value.CreatedOn < DateTime.Now.AddMinutes(CleanupFrequencyMinutes * -1)).ToArray();
            foreach (var oldGame in oldGames)
            {
                logger.LogInformation("Cleaning up game {gameId}", oldGame.Key);
                multiGameHoster.Games.Remove(oldGame.Key, out _);
            }

            if (oldGames.Any())
            {
                multiGameHoster.RaiseOldGamesPurged();
            }
        }
    }
}
