using System.Text.Json;

namespace A2450ReportLogger;

internal static class SimulatedReportRun
{
    public static void WriteLogs()
    {
        Directory.CreateDirectory("logs");

        var path = Path.Combine("logs", "a2450-report-simulated.jsonl");
        var devicePath = @"\\?\hid#simulated_vid_0001004c_pid_029c_col02";
        var now = DateTimeOffset.Now;

        var reports = new[]
        {
            new byte[] { 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x01, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x01, 0x70, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00 }
        };

        var lines = reports.Select((report, index) => JsonSerializer.Serialize(
            new ReportRecord(
                Time: now.AddMilliseconds(index * 120),
                DevicePath: devicePath,
                ReportLength: report.Length,
                ReportHex: Hex.ToHex(report),
                IsDuplicate: index > 0 && report.SequenceEqual(reports[index - 1])
            )));

        File.WriteAllLines(path, lines);

        Console.WriteLine("A2450 report logger simulation complete.");
        Console.WriteLine("Generated: logs/a2450-report-simulated.jsonl");
        Console.WriteLine("This is only a format test, not real keyboard data.");
    }
}
