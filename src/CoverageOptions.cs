namespace PrintCoverageAnalyzer;

public sealed class CoverageOptions
{
    /// <summary>
    /// RGB values >= this threshold are considered white-ish.
    /// </summary>
    public byte WhiteThreshold { get; init; } = 245;

    /// <summary>
    /// Alpha values <= this threshold are treated as non-printed (transparent background).
    /// </summary>
    public byte TransparentThreshold { get; init; } = 8;

    /// <summary>
    /// Rendering dpi for PDF pages.
    /// </summary>
    public int PdfDpi { get; init; } = 300;
}
