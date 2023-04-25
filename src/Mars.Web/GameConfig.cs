namespace Mars.Web;

public class GameConfig
{
	public string Password { get; set; } = "password";
	public int ApiLimitPerSecond { get; set; } = 50;
	public int MaxMaps { get; set; } = 2;
	public int DefaultMap { get; set; } = 0;
	public int CleanupFrequencyMinutes { get; set; } = 60;
}
