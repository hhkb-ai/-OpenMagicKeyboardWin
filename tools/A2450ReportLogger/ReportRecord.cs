namespace A2450ReportLogger;

internal sealed record ReportRecord(
    DateTimeOffset Time,
    string DevicePath,
    int ReportLength,
    string ReportHex,
    bool IsDuplicate
);
