using System.Runtime.InteropServices;
using System.Text.Json;
using A2450ReportLogger;
using static A2450ReportLogger.HidNative;

// --- Simulate mode ---
if (args.Contains("--simulate"))
{
    SimulatedReportRun.WriteLogs();
    return;
}

// --- HidSharp diagnostic mode ---
if (args.Contains("--hidsharp"))
{
    HidSharpDiag.Run();
    return;
}

// --- Real HID Report reading mode ---

// Check for --col01 flag to read from COL01 instead of COL02
bool readCol01 = args.Contains("--col01");
string targetCol = readCol01 ? "COL01" : "COL02";

Console.WriteLine("[INFO] A2450ReportLogger - Raw HID Report Reader");
Console.WriteLine($"[INFO] Looking for Apple Magic Keyboard A2450 {targetCol}...");
Console.WriteLine();

// 1. Find device paths
var paths = readCol01
    ? HidDeviceEnumerator.FindA2450Col01Paths()
    : HidDeviceEnumerator.FindA2450Col02Paths();

if (paths.Count == 0)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("[FATAL] No COL02 device path found. Cannot proceed.");
    Console.Error.WriteLine("        Make sure the keyboard is connected and paired.");
    return;
}

string devicePath = paths[0];

// For COL01, strip the \kbd suffix to access the base HID interface
if (readCol01 && devicePath.EndsWith("\\kbd", StringComparison.OrdinalIgnoreCase))
{
    devicePath = devicePath[..^4];
    Console.WriteLine($"[INFO] Stripped \\kbd suffix from COL01 path.");
}

Console.WriteLine();
Console.WriteLine($"[INFO] Opening: {devicePath}");

// 2. Open the HID device (synchronous, no overlapped)
IntPtr hDevice = CreateFile(
    devicePath,
    GENERIC_READ,
    FILE_SHARE_READ | FILE_SHARE_WRITE,
    IntPtr.Zero,
    OPEN_EXISTING,
    FILE_ATTRIBUTE_NORMAL,
    IntPtr.Zero);

if (hDevice == IntPtr.Zero || hDevice == new IntPtr((long)INVALID_HANDLE_VALUE))
{
    int err = Marshal.GetLastWin32Error();
    Console.Error.WriteLine($"[FATAL] CreateFile failed, Win32 error={err}");
    Console.Error.WriteLine($"        0x{err:X4} = {GetErrorMessage(err)}");
    Console.Error.WriteLine("        Try running as Administrator if access is denied.");
    return;
}

Console.WriteLine("[INFO] Device opened successfully.");

// 3. Get preparsed data and capabilities
IntPtr preparsedData = IntPtr.Zero;
if (!HidD_GetPreparsedData(hDevice, out preparsedData))
{
    int err = Marshal.GetLastWin32Error();
    Console.Error.WriteLine($"[FATAL] HidD_GetPreparsedData failed, error={err}");
    CloseHandle(hDevice);
    return;
}

var caps = new HIDP_CAPS();
int status = HidP_GetCaps(preparsedData, out caps);
HidD_FreePreparsedData(preparsedData);

// HIDP_STATUS_SUCCESS = 0x00110000
const int HIDP_STATUS_SUCCESS = 0x00110000;
if (status != HIDP_STATUS_SUCCESS)
{
    Console.Error.WriteLine($"[FATAL] HidP_GetCaps failed, status=0x{status:X8}");
    CloseHandle(hDevice);
    return;
}

int reportLength = caps.InputReportByteLength;
Console.WriteLine($"[INFO] InputReportByteLength = {reportLength}");
Console.WriteLine($"[INFO] UsagePage = 0x{caps.UsagePage:X4}, Usage = 0x{caps.Usage:X4}");

if (reportLength == 0)
{
    Console.Error.WriteLine("[FATAL] InputReportByteLength is 0, cannot read reports.");
    CloseHandle(hDevice);
    return;
}

// 4. Prepare for synchronous ReadFile
Directory.CreateDirectory("logs");
string outPath = Path.Combine("logs", "a2450-raw-hid-reports.jsonl");
Console.WriteLine($"[INFO] Writing to: {outPath}");
Console.WriteLine("[INFO] Mode: synchronous ReadFile (blocking)");
Console.WriteLine("[INFO] Press Ctrl+C to stop.");
Console.WriteLine();

var jsonOpts = new JsonSerializerOptions { WriteIndented = false };
string? prevHex = null;
int reportCount = 0;
int dupCount = 0;

// Set up Ctrl+C handler
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine();
    Console.WriteLine("[INFO] Stopping...");
    cts.Cancel();
};

// 5. Read loop - synchronous ReadFile (blocks until report arrives)
using var writer = new StreamWriter(outPath);
byte[] buffer = new byte[reportLength];

Console.WriteLine("[INFO] Waiting for HID reports... (press keys on the keyboard)");

while (!cts.Token.IsCancellationRequested)
{
    // Synchronous ReadFile - blocks until a report is available
    // The overlapped struct is ignored in synchronous mode
    var overlapped = new OVERLAPPED();
    bool ok = ReadFile(hDevice, buffer, reportLength, out int bytesRead, ref overlapped);

    if (!ok)
    {
        int err = Marshal.GetLastWin32Error();
        Console.Error.WriteLine($"[ERROR] ReadFile failed, error={err} (0x{err:X4})");
        if (cts.Token.IsCancellationRequested) break;
        Thread.Sleep(100);
        continue;
    }

    if (bytesRead == 0) continue;

    // Process the report
    reportCount++;
    string hex = Hex.ToString(buffer, bytesRead);
    bool isDup = hex == prevHex;

    if (isDup) dupCount++;

    var record = new ReportRecord(
        Time: DateTimeOffset.Now,
        DevicePath: devicePath,
        ReportLength: bytesRead,
        ReportHex: hex,
        IsDuplicate: isDup
    );

    writer.WriteLine(JsonSerializer.Serialize(record, jsonOpts));
    writer.Flush();

    // Console output
    string dupTag = isDup ? " [DUP]" : "";
    Console.WriteLine($"  [{reportCount,5}] {hex}{dupTag}");

    prevHex = hex;
}

CloseHandle(hDevice);

Console.WriteLine();
Console.WriteLine($"[INFO] Done. Total reports: {reportCount}, duplicates: {dupCount}");
Console.WriteLine($"[INFO] Output: {outPath}");

static string GetErrorMessage(int errorCode) => errorCode switch
{
    2 => "ERROR_FILE_NOT_FOUND - Device path not found",
    5 => "ERROR_ACCESS_DENIED - Access denied (try running as Admin)",
    6 => "ERROR_INVALID_HANDLE - Invalid handle",
    31 => "ERROR_GEN_FAILURE - Device not responding",
    161 => "ERROR_BAD_PATHNAME - Invalid device path",
    _ => $"Win32 error {errorCode}"
};
