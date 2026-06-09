namespace A2450ReportLogger;

public sealed record ReportRecord(
    DateTimeOffset Time,
    string DevicePath,
    int ReportLength,
    string ReportHex,
    bool IsDuplicate
);
