using Microsoft.Extensions.Logging;

namespace Mars.MissionControl.Tests;

public static class TestLogger
{
    private static ILoggerFactory loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
    public static ILogger<Game> MakeNewGameLogger() => loggerFactory.CreateLogger<Game>();
}
