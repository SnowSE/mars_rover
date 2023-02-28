using System.Diagnostics;

namespace Mars.Web;

public static class GameActivitySources
{
    public static ActivitySource Instance { get; } = new("Game");
}

public static class JoinActivitySources
{
    public static ActivitySource Instance { get; } = new("Join");
}

public static class StatusActivitySources
{
    public static ActivitySource Instance { get; } = new("Status");
}