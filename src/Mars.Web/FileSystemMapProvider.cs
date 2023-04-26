using OfficeOpenXml;
using System.Text.Json;

namespace Mars.Web;

public interface IMapProvider
{
	IEnumerable<Map> LoadMaps();
}

public partial class FileSystemMapProvider : IMapProvider
{
	private readonly IWebHostEnvironment hostEnvironment;
	private readonly ILogger<FileSystemMapProvider> logger;

	public FileSystemMapProvider(IWebHostEnvironment hostEnvironment, ILogger<FileSystemMapProvider> logger)
	{
		this.hostEnvironment = hostEnvironment;
		this.logger = logger;
	}

	[LoggerMessage(1, LogLevel.Information, "Processing map file {filePath}")] partial void LogProcessingMapFile(string filePath);
	[LoggerMessage(2, LogLevel.Information, "Beginning to process {excelFile}")] partial void LogProcessingExcelFile(string excelFile);

	public IEnumerable<Map> LoadMaps()
	{
		var imagesFolder = Path.Combine(hostEnvironment.WebRootPath, "..", "data");

		List<Map> maps = new();
		var terrainFiles = Directory.GetFiles(imagesFolder, "terrain_*.json");
		foreach (var file in terrainFiles)
		{
			LogProcessingMapFile(file);

			var parts = Path.GetFileName(file).Split('_', '.');
			var mapNumber = int.Parse(parts[1]);
			var content = File.ReadAllText(file);
			var json = (JsonSerializer.Deserialize<IEnumerable<IEnumerable<int>>>(content) ?? throw new UnableToDeserializeTerrainException()).ToList();
			var cells = new List<MissionControl.Cell>();
			for (int row = 0; row < json.Count; row++)
			{
				var cellsInRow = json[row].ToList();
				for (int col = 0; col < cellsInRow.Count; col++)
				{
					cells.Add(new MissionControl.Cell(new MissionControl.Location(row, col), new Difficulty(cellsInRow[col])));
				}
			}

			var lowResCachedPath = Path.Combine(imagesFolder, $"lowres_{Path.GetFileName(file)}");
			var lowRes = (File.Exists(lowResCachedPath)) ?
				parseLowResolutionMap(lowResCachedPath) :
				fillLowResoulutionMap(cells, lowResCachedPath);
			maps.Add(new Map(mapNumber, cells, lowRes));
		}

		loadExcelMaps(imagesFolder, maps);

		return maps;
	}

	private void loadExcelMaps(string imagesFolder, List<Map> maps)
	{
		ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

		foreach (var excelFile in Directory.GetFiles(imagesFolder, "terrain_*.xlsx", SearchOption.AllDirectories))
		{
			LogProcessingExcelFile(excelFile);
			using var package = new ExcelPackage(new FileInfo(excelFile));
			var sheet = package.Workbook.Worksheets["rawValues"];
			if (sheet == null)
			{
				logger.LogError("Excel file {excelFile} doesn't have a 'rawValues' sheet!", excelFile);
				continue;
			}

			var cells = new List<MissionControl.Cell>();

			for (int excelRow = 500, mapRow = 0; excelRow > 0; excelRow--, mapRow++)
			{
				for (int excelCol = 1, mapCol = 0; excelCol <= 500; excelCol++, mapCol++)
				{
					var difficultyValue = (long)(double)sheet.Cells[excelRow, excelCol].Value;
					if (difficultyValue > int.MaxValue)
						difficultyValue = int.MaxValue;
					cells.Add(new MissionControl.Cell(new MissionControl.Location(mapCol, mapRow), new Difficulty((int)difficultyValue)));
				}
			}

			var lowResCachedPath = Path.Combine(imagesFolder, $"lowres_{Path.GetFileNameWithoutExtension(excelFile)}.json");
			var lowRes = (File.Exists(lowResCachedPath)) ?
				parseLowResolutionMap(lowResCachedPath) :
				fillLowResoulutionMap(cells, lowResCachedPath);
			var parts = Path.GetFileName(excelFile).Split('_', '.');
			var mapNumber = int.Parse(parts[1]);
			maps.Add(new Map(mapNumber, cells, lowRes));
		}
	}

	private List<LowResolutionCell> parseLowResolutionMap(string lowResCachedPath)
	{
		logger.LogInformation("Deserializing previously parsed low-res map.");

		var content = File.ReadAllText(lowResCachedPath);
		var serializedTiles = JsonSerializer.Deserialize<IEnumerable<SerializedLowResolutionCell>>(content) ?? throw new UnableToDeserializeLowResolutionCellException();
		return new(serializedTiles.Select(t =>
			new LowResolutionCell(t.AverageDifficulty, t.LowerLeftRow, t.LowerLeftColumn, t.UpperRightRow, t.UpperRightColumn)
		));
	}

	private List<LowResolutionCell> fillLowResoulutionMap(List<Mars.MissionControl.Cell> cells, string lowResCachedPath)
	{
		logger.LogInformation("Parsing low-res map...");
		var lowResTiles = Helpers.BuildLowResMap(cells);
		var serializableTiles = lowResTiles.Select(t => SerializedLowResolutionCell.FromLowResCel(t));

		logger.LogInformation("Serializing low res map to disk {file}", lowResCachedPath);
		File.WriteAllText(lowResCachedPath, JsonSerializer.Serialize(serializableTiles));
		return lowResTiles;
	}
}