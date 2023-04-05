using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Mars.MissionControl.Tests;

public class GameScenario
{
    public GameScenario(int height, int width, int players, int cellDifficulty = 1, int startingBatteryLevel = 18_000, ILogger<Game> customLogger = null, IEnumerable<Location> targets = null)
    {
        Game = Helpers.CreateGame(height, width, cellDifficulty: cellDifficulty, startingBatteryLevel: startingBatteryLevel, customLogger: customLogger, targets: targets);
        Players = new(Enumerable.Range(0, players)
                                .Select(p => Game.Join($"Player{p}"))
                                .ToArray());
    }

    public Game Game { get; private set; }

    public List<JoinResult> Players { get; private set; }
}
