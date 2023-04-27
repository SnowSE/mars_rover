using System.ComponentModel.DataAnnotations;

namespace Mars.Web;

public class GameConfig
{
	[Required(AllowEmptyStrings = false)]
	public string Password { get; set; } = "password";
	[Range(10, 100_000)]
	public int ApiLimitPerSecond { get; set; } = 50;
	public int MaxMaps { get; set; } = 2;
	public int DefaultMap { get; set; } = 0;
	public int CleanupFrequencyMinutes { get; set; } = 60;
}
