using System.Diagnostics;

namespace Mars.Web;

public static class ActivitySources
{
    public static ActivitySource MarsWeb { get; } = new ActivitySource("Mars.Web", "1.0");
    public static ActivitySource Demo { get; } = new ActivitySource("Demo");
}