using System.CommandLine;
using System.Text.Json;
using PrintCoverageAnalyzer;

var inputArgument = new Argument<FileInfo[]>(
    name: "input",
    description: "Input files (.jpg/.png/.pdf etc.)")
{
    Arity = ArgumentArity.OneOrMore,
};

var whiteThresholdOption = new Option<byte>(
    aliases: ["--white-threshold"],
    getDefaultValue: () => 245,
    description: "RGB threshold from which a pixel is treated as white.");

var alphaThresholdOption = new Option<byte>(
    aliases: ["--alpha-threshold"],
    getDefaultValue: () => 8,
    description: "Alpha threshold at/below which a pixel is treated as transparent.");

var pdfDpiOption = new Option<int>(
    aliases: ["--pdf-dpi"],
    getDefaultValue: () => 300,
    description: "Rasterization DPI for PDF pages.");

var jsonOption = new Option<FileInfo?>(
    aliases: ["--json-out"],
    description: "Optional path to write JSON report.");

var root = new RootCommand("Berechnet bedruckte Fläche (alles außer weiß) für Bilder/PDFs.");
root.AddArgument(inputArgument);
root.AddOption(whiteThresholdOption);
root.AddOption(alphaThresholdOption);
root.AddOption(pdfDpiOption);
root.AddOption(jsonOption);

root.SetHandler((FileInfo[] files, byte whiteThreshold, byte alphaThreshold, int pdfDpi, FileInfo? jsonOut) =>
{
    var options = new CoverageOptions
    {
        WhiteThreshold = whiteThreshold,
        TransparentThreshold = alphaThreshold,
        PdfDpi = pdfDpi,
    };

    var results = new List<AnalysisResult>();

    foreach (var file in files)
    {
        if (!file.Exists)
        {
            Console.Error.WriteLine($"Datei nicht gefunden: {file.FullName}");
            continue;
        }

        foreach (var (name, image) in InputLoader.Load(file.FullName, options))
        {
            using (image)
            {
                var result = CoverageAnalyzer.Analyze(image, name, options);
                results.Add(result);
            }
        }
    }

    foreach (var r in results)
    {
        Console.WriteLine($"[{r.SourceFile}] {r.Width}x{r.Height}");
        Console.WriteLine($"  Bedruckt: {r.PrintedPercent:F2}% ({r.PrintedPixels:N0}/{r.TotalPixels:N0} Pixel)");
        Console.WriteLine($"  CMYK-Schätzung: C={r.CyanPercent:F2}% M={r.MagentaPercent:F2}% Y={r.YellowPercent:F2}% K={r.BlackPercent:F2}% | Ø={r.CmykInkEstimatePercent:F2}%");
    }

    if (jsonOut is not null)
    {
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonOut.FullName, json);
        Console.WriteLine($"JSON-Report geschrieben: {jsonOut.FullName}");
    }
});

return await root.InvokeAsync(args);
