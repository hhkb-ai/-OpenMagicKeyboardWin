using System.Text.Json;
using OpenMagicKeyboard.Shared;

namespace A2450HidLogger;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("A2450HidLogger only runs on Windows.");
            return;
        }

        Directory.CreateDirectory("logs");

        var devices = DeviceEnumerator.EnumerateKeyboardLikeDevices().ToList();
        File.WriteAllText(
            Path.Combine("logs", "device-snapshot.json"),
            JsonSerializer.Serialize(devices, JsonOptions.Default));

        Console.WriteLine("OpenMagicKeyboardWin - A2450 HID Logger");
        Console.WriteLine("Logs will be written to logs/a2450-key-events.jsonl");
        Console.WriteLine();
        Console.WriteLine("Detected keyboard-like devices:");

        foreach (var device in devices)
        {
            var marker = device.LooksLikeAppleKeyboard ? "[APPLE?]" : "        ";
            Console.WriteLine($"{marker} {device.Name} | VID={device.Vid ?? "?"} PID={device.Pid ?? "?"}");
            Console.WriteLine($"          {device.DeviceId}");
        }

        Console.WriteLine();
        Console.WriteLine("Test matrix:");
        Console.WriteLine("  Fn, Left Ctrl, Right Ctrl, Fn+Ctrl, F1-F12, Fn+F1-F12");
        Console.WriteLine("  Fn+Backspace, Fn+Arrows, Command, Option, Lock/Eject");
        Console.WriteLine();
        Console.WriteLine("Close the small logger window to stop.");

        ApplicationConfiguration.Initialize();
        using var sink = new RawInputSink();
        Application.Run(sink);
    }
}
