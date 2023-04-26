namespace Mars.MissionControl;

public class MapWithTargets
{
	public MapWithTargets(Map map, string targets)
	{
		Map = map;
		Targets = ParseLocations(targets) ?? throw new UnableToParseTargetsException();
	}

	public static IEnumerable<Location>? ParseLocations(string locations)
	{
		try
		{
			return (from word in locations.Split(';', StringSplitOptions.RemoveEmptyEntries)
					let parts = word.Split(new[] { '(', ',', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries)
					let x = int.Parse(parts[0])
					let y = int.Parse(parts[1])
					select new Location(x, y)).ToArray();
		}
		catch
		{
			throw new InvalidFormatException("Locations must be in a (x,y);(x1,y1) format");
		}
	}

	public MapWithTargets(Map map, IEnumerable<Location> targets)
	{
		Map = map;
		Targets = targets;
	}

	public Map Map { get; }
	public IEnumerable<Location> Targets { get; }
}
