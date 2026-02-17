using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PrintCoverageAnalyzer;

public static class CoverageAnalyzer
{
    public static AnalysisResult Analyze(Image<Rgba32> image, string sourceName, CoverageOptions options)
    {
        long totalPixels = (long)image.Width * image.Height;
        long printedPixels = 0;

        double cyanCoverage = 0;
        double magentaCoverage = 0;
        double yellowCoverage = 0;
        double blackCoverage = 0;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    Rgba32 pixel = row[x];

                    if (pixel.A <= options.TransparentThreshold)
                    {
                        continue;
                    }

                    bool isWhiteLike = pixel.R >= options.WhiteThreshold
                        && pixel.G >= options.WhiteThreshold
                        && pixel.B >= options.WhiteThreshold;

                    if (!isWhiteLike)
                    {
                        printedPixels++;
                    }

                    var (c, m, yk, k) = RgbToCmyk(pixel);
                    cyanCoverage += c;
                    magentaCoverage += m;
                    yellowCoverage += yk;
                    blackCoverage += k;
                }
            }
        });

        double printedPercent = totalPixels == 0 ? 0 : printedPixels * 100.0 / totalPixels;

        double cPercent = totalPixels == 0 ? 0 : cyanCoverage * 100.0 / totalPixels;
        double mPercent = totalPixels == 0 ? 0 : magentaCoverage * 100.0 / totalPixels;
        double yPercent = totalPixels == 0 ? 0 : yellowCoverage * 100.0 / totalPixels;
        double kPercent = totalPixels == 0 ? 0 : blackCoverage * 100.0 / totalPixels;
        double inkEstimate = (cPercent + mPercent + yPercent + kPercent) / 4.0;

        return new AnalysisResult(
            sourceName,
            image.Width,
            image.Height,
            totalPixels,
            printedPixels,
            printedPercent,
            inkEstimate,
            cPercent,
            mPercent,
            yPercent,
            kPercent);
    }

    private static (double C, double M, double Y, double K) RgbToCmyk(Rgba32 rgba)
    {
        // Normalize
        double r = rgba.R / 255.0;
        double g = rgba.G / 255.0;
        double b = rgba.B / 255.0;

        double k = 1 - Math.Max(r, Math.Max(g, b));
        if (Math.Abs(k - 1.0) < 1e-9)
        {
            return (0, 0, 0, 1);
        }

        double c = (1 - r - k) / (1 - k);
        double m = (1 - g - k) / (1 - k);
        double y = (1 - b - k) / (1 - k);

        return (Clamp01(c), Clamp01(m), Clamp01(y), Clamp01(k));
    }

    private static double Clamp01(double value) => value switch
    {
        < 0 => 0,
        > 1 => 1,
        _ => value,
    };
}
