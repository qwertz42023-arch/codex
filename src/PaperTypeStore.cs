using System.Text.Json;

namespace PrintCoverageAnalyzer;

public sealed class PaperTypeStore
{
    private readonly string _filePath;

    public PaperTypeStore()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string dir = Path.Combine(appData, "PrintCoverageAnalyzer");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "paper-types.json");
    }

    public List<PaperType> Load()
    {
        if (!File.Exists(_filePath))
        {
            var defaults = DefaultPaperTypes();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(_filePath);
        var items = JsonSerializer.Deserialize<List<PaperType>>(json);

        if (items is null || items.Count == 0)
        {
            items = DefaultPaperTypes();
            Save(items);
        }

        return items;
    }

    public void Save(List<PaperType> items)
    {
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private static List<PaperType> DefaultPaperTypes() =>
    [
        new("Standard matt 150g", 9.90m),
        new("Premium seidenmatt 200g", 12.50m),
        new("Outdoor/Poster 120g", 10.80m),
        new("FineArt 230g", 18.90m),
    ];
}
