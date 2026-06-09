using HidSharp;

namespace A2450ReportLogger;

internal static class HidSharpDiag
{
    public static void Run()
    {
        Console.WriteLine("[INFO] HidSharp Diagnostic Mode");
        Console.WriteLine("[INFO] Enumerating all HID devices...");
        Console.WriteLine();

        // Find all Apple A2450 devices
        var allDevices = DeviceList.Local.GetHidDevices().ToList();
        Console.WriteLine($"[INFO] Total HID devices: {allDevices.Count}");

        // Show all devices with Apple VID (Bluetooth 004C or USB 05AC)
        var appleDevices = allDevices
            .Where(d => (d.VendorID == 0x004C || d.VendorID == 0x05AC) && d.ProductID == 0x029C)
            .ToList();

        Console.WriteLine($"[INFO] Apple A2450 devices (VID 004C/05AC, PID 029C): {appleDevices.Count}");

        // Also show any new keyboard-like devices
        var keyboardDevices = allDevices
            .Where(d => d.GetMaxInputReportLength() >= 8)
            .ToList();

        Console.WriteLine($"[INFO] Keyboard-like devices (input>=8): {keyboardDevices.Count}");
        Console.WriteLine();

        // Show ALL devices for debugging
        Console.WriteLine("[INFO] All HID devices:");
        foreach (var dev in allDevices)
        {
            Console.WriteLine($"  VID=0x{dev.VendorID:X4} PID=0x{dev.ProductID:X4} InMax={dev.GetMaxInputReportLength()} Path=...{dev.DevicePath[^60..]}");
        }
        Console.WriteLine();

        foreach (var dev in appleDevices)
        {
            Console.WriteLine($"  Path: {dev.DevicePath}");
            Console.WriteLine($"  VID=0x{dev.VendorID:X4} PID=0x{dev.ProductID:X4}");
            Console.WriteLine($"  MaxInputReportLength={dev.GetMaxInputReportLength()}");
            Console.WriteLine($"  MaxOutputReportLength={dev.GetMaxOutputReportLength()}");
            Console.WriteLine($"  MaxFeatureReportLength={dev.GetMaxFeatureReportLength()}");
            Console.WriteLine();
        }

        // Try to open each Apple device and read
        foreach (var dev in appleDevices)
        {
            string pathLower = dev.DevicePath.ToLowerInvariant();
            string col = pathLower.Contains("col02") ? "COL02" :
                         pathLower.Contains("col01") ? "COL01" : "BASE";

            Console.WriteLine($"[INFO] Trying to open {col}: {dev.DevicePath}");

            try
            {
                using var stream = dev.Open();
                Console.WriteLine($"[INFO]   Opened successfully!");
                Console.WriteLine($"[INFO]   Waiting 5 seconds for reports... (press Fn keys now)");

                stream.ReadTimeout = 5000;

                int reportCount = 0;
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            byte[] buffer = new byte[dev.GetMaxInputReportLength()];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);

                            if (bytesRead > 0)
                            {
                                reportCount++;
                                string hex = Hex.ToString(buffer, bytesRead);
                                Console.WriteLine($"  [{reportCount,5}] {bytesRead} bytes: {hex}");
                            }
                        }
                        catch (TimeoutException)
                        {
                            Console.WriteLine($"[INFO]   Timeout - no reports received in 5 seconds.");
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"[INFO]   Timeout after 10 seconds.");
                }

                Console.WriteLine($"[INFO]   Total reports from {col}: {reportCount}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR]  Failed to open {col}: {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}
