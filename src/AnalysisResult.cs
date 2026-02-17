namespace PrintCoverageAnalyzer;

public sealed record AnalysisResult(
    string SourceFile,
    int Width,
    int Height,
    long TotalPixels,
    long PrintedPixels,
    double PrintedPercent,
    double CmykInkEstimatePercent,
    double CyanPercent,
    double MagentaPercent,
    double YellowPercent,
    double BlackPercent);
