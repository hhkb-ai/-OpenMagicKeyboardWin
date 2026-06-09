using System.Text.Json;

namespace A2450ReportLogger;

internal static class SimulatedReportRun
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false
    };

    public static void WriteLogs()
    {
        Directory.CreateDirectory("logs");

        // Simulated COL02 device path
        const string devicePath = @"\\?\HID#VID_004C&PID_029C&COL02#7&abcdef0&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}";

        // Simulated HID reports (fake data, not real keyboard data)
        var reports = new List<(byte[] data, bool dup)>
        {
            // Idle state (all zeros except report ID)
            (new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, false),
            // Fn down (simulated)
            (new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, false),
            // Fn still held (duplicate-like)
            (new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, true),
            // Fn up (back to idle)
            (new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, false),
            // Fn + F1 (simulated)
            (new byte[] { 0x01, 0x01, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00 }, false),
            // Fn + F1 release
            (new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, false),
        };

        string outPath = Path.Combine("logs", "a2450-report-simulated.jsonl");
        using var writer = new StreamWriter(outPath);

        var time = DateTimeOffset.Now;
        string? prevHex = null;

        foreach (var (data, forcedDup) in reports)
        {
            string hex = Hex.ToString(data, data.Length);
            bool isDup = forcedDup || (hex == prevHex);

            var record = new ReportRecord(
                Time: time,
                DevicePath: devicePath,
                ReportLength: data.Length,
                ReportHex: hex,
                IsDuplicate: isDup
            );

            writer.WriteLine(JsonSerializer.Serialize(record, JsonOpts));
            time += TimeSpan.FromMilliseconds(120);
            prevHex = hex;
        }

        Console.WriteLine("Simulation complete.");
        Console.WriteLine($"Generated: {outPath}");
    }
}
