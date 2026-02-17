using PdfiumViewer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PrintCoverageAnalyzer;

public static class InputLoader
{
    private static readonly HashSet<string> RasterExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".bmp", ".webp"
    };

    public static IEnumerable<(string Name, SixLabors.ImageSharp.Image<Rgba32> Image)> Load(string path, CoverageOptions options)
    {
        string ext = Path.GetExtension(path);

        if (RasterExtensions.Contains(ext))
        {
            yield return (Path.GetFileName(path), SixLabors.ImageSharp.Image.Load<Rgba32>(path));
            yield break;
        }

        if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in LoadPdf(path, options.PdfDpi))
            {
                yield return item;
            }

            yield break;
        }

        throw new NotSupportedException($"Unsupported format: {ext}");
    }

    private static IEnumerable<(string Name, SixLabors.ImageSharp.Image<Rgba32> Image)> LoadPdf(string pdfPath, int dpi)
    {
        using var document = PdfDocument.Load(pdfPath);
        for (int i = 0; i < document.PageCount; i++)
        {
            var size = document.PageSizes[i];
            int width = (int)Math.Round(size.Width / 72.0 * dpi);
            int height = (int)Math.Round(size.Height / 72.0 * dpi);

            using var bitmap = document.Render(i, width, height, dpi, dpi, PdfRenderFlags.Annotations);
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            yield return ($"{Path.GetFileName(pdfPath)}#page-{i + 1}", SixLabors.ImageSharp.Image.Load<Rgba32>(ms));
        }
    }
}
