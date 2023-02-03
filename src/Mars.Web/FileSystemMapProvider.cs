using System.Text.Json;

namespace Mars.Web;

public interface IMapProvider
{
    IEnumerable<Map> LoadMaps();
}

public class FileSystemMapProvider : IMapProvider
{
    private readonly IWebHostEnvironment hostEnvironment;

    public FileSystemMapProvider(IWebHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;
    }

    public IEnumerable<Map> LoadMaps()
    {
        var imagesFolder = Path.Combine(hostEnvironment.WebRootPath, "images");

        List<Map> maps = new ();
        var terrainFiles = Directory.GetFiles(imagesFolder, "terrain_*.json");
        foreach (var file in terrainFiles)
        {
            var parts = Path.GetFileName(file).Split('_', '.');
            var mapNumber = int.Parse(parts[1]);
            var content = File.ReadAllText(file);
            var json = JsonSerializer.Deserialize<IEnumerable<IEnumerable<int>>>(content).ToList();
            var cells = new List<Mars.MissionControl.Cell>();
            for (int row = 0; row < json.Count; row++)
            {
                var cellsInRow = json[row].ToList();
                for (int col = 0; col < cellsInRow.Count; col++)
                {
                    cells.Add(new Mars.MissionControl.Cell(new Location(row, col), new Difficulty(cellsInRow[col])));
                }
            }

            var lowResCachedPath = Path.Combine(imagesFolder, Path.ChangeExtension(file, ".lowres.json"));
            var lowRes = (File.Exists(lowResCachedPath)) ?
                parseLowResolutionMap(lowResCachedPath) :
                fillLowResoulutionMap(cells, lowResCachedPath);
            maps.Add(new Map(mapNumber, cells, lowRes));
        }

        return maps;
    }

    private List<LowResolutionCell> parseLowResolutionMap(string lowResCachedPath)
    {
        var content = File.ReadAllText(lowResCachedPath);
        var serializedTiles = JsonSerializer.Deserialize<IEnumerable<SerializedLowResolutionCell>>(content);
        return new(serializedTiles.Select(t => new LowResolutionCell(t.AverageDifficulty, t.LowerLeftRow, t.LowerLeftColumn, t.UpperRightRow, t.UpperRightColumn)));
    }

    private List<LowResolutionCell> fillLowResoulutionMap(List<Mars.MissionControl.Cell> cells, string lowResCachedPath)
    {
        var lowResTiles = Helpers.BuildLowResMap(cells);
        var serializableTiles = lowResTiles.Select(t => SerializedLowResolutionCell.FromLowResCel(t));
        File.WriteAllText(lowResCachedPath, JsonSerializer.Serialize(serializableTiles));
        return lowResTiles;
    }
}