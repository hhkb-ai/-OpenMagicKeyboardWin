using System.Runtime.InteropServices;
using static A2450ReportLogger.HidNative;

namespace A2450ReportLogger;

internal static class HidDeviceEnumerator
{
    /// <summary>
    /// Enumerate HID device interfaces and return paths matching the Apple A2450 COL01 filter.
    /// </summary>
    public static List<string> FindA2450Col01Paths()
    {
        return FindA2450Paths("col01");
    }

    /// <summary>
    /// Enumerate HID device interfaces and return paths matching the Apple A2450 COL02 filter.
    /// </summary>
    public static List<string> FindA2450Col02Paths()
    {
        return FindA2450Paths("col02");
    }

    private static List<string> FindA2450Paths(string colFilter)
    {
        var results = new List<string>();
        var allA2450 = new List<string>();

        IntPtr deviceInfoSet = SetupDiGetClassDevs(
            ref HidGuid, null, IntPtr.Zero,
            DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

        if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == new IntPtr(-1))
        {
            Console.Error.WriteLine("[ERROR] SetupDiGetClassDevs failed.");
            return results;
        }

        try
        {
            var ifaceData = new SP_DEVICE_INTERFACE_DATA();
            ifaceData.cbSize = Marshal.SizeOf(ifaceData);

            for (int i = 0; ; i++)
            {
                if (!SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero,
                    ref HidGuid, i, ref ifaceData))
                {
                    int err = Marshal.GetLastWin32Error();
                    if (err == ERROR_NO_MORE_ITEMS) break;
                    Console.Error.WriteLine($"[WARN] SetupDiEnumDeviceInterfaces[{i}] failed, error={err}");
                    continue;
                }

                // Get required size first
                SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref ifaceData,
                    IntPtr.Zero, 0, out int requiredSize, IntPtr.Zero);

                // Use struct-based marshaling
                var detailData = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                // On x86: cbSize = 4 + sizeof(WCHAR) = 6
                // On x64: cbSize = 4 + 4 (alignment padding) = 8
                detailData.cbSize = Environment.Is64BitProcess ? 8 : 6;

                if (!SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref ifaceData,
                    ref detailData, requiredSize, out _, IntPtr.Zero))
                {
                    int err = Marshal.GetLastWin32Error();
                    Console.Error.WriteLine($"[WARN] SetupDiGetDeviceInterfaceDetail[{i}] failed, error={err}");
                    continue;
                }

                string devicePath = detailData.DevicePath;
                string lower = devicePath.ToLowerInvariant();

                // Check for Apple A2450 (VID 004C, PID 029C)
                if (lower.Contains("vid&0001004c") && lower.Contains("pid&029c"))
                {
                    allA2450.Add(devicePath);

                    if (lower.Contains(colFilter))
                    {
                        results.Add(devicePath);
                    }
                }
            }
        }
        finally
        {
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }

        // Diagnostic output
        if (allA2450.Count > 0)
        {
            Console.WriteLine($"[INFO] Found {allA2450.Count} Apple A2450 HID interface(s):");
            foreach (var p in allA2450)
            {
                string tag = p.ToLowerInvariant().Contains(colFilter) ? $" <-- {colFilter.ToUpper()}" : "";
                Console.WriteLine($"       {p}{tag}");
            }
        }

        if (results.Count == 0 && allA2450.Count == 0)
        {
            Console.Error.WriteLine("[WARN] No Apple A2450 HID interfaces found at all.");
            Console.Error.WriteLine("       Is the keyboard connected via Bluetooth?");
        }
        else if (results.Count == 0)
        {
            Console.Error.WriteLine($"[WARN] Found Apple A2450 interfaces but none with {colFilter.ToUpper()}.");
            Console.Error.WriteLine("       The keyboard may need to be re-paired.");
        }

        return results;
    }
}
