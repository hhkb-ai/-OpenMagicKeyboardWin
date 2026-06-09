using System.Management;
using System.Text.RegularExpressions;
using OpenMagicKeyboard.Shared;

namespace A2450HidLogger;

internal static class DeviceEnumerator
{
    private static readonly Regex VidRegex = new(@"VID[_&]([0-9A-Fa-f]{4})", RegexOptions.Compiled);
    private static readonly Regex PidRegex = new(@"PID[_&]([0-9A-Fa-f]{4})", RegexOptions.Compiled);

    public static IEnumerable<KeyboardDeviceInfo> EnumerateKeyboardLikeDevices()
    {
        var query = "SELECT DeviceID, Name, Manufacturer, PNPClass, Service FROM Win32_PnPEntity";
        using var searcher = new ManagementObjectSearcher(query);

        foreach (var item in searcher.Get().Cast<ManagementObject>())
        {
            var deviceId = Convert.ToString(item["DeviceID"]) ?? string.Empty;
            var name = Convert.ToString(item["Name"]) ?? string.Empty;
            var manufacturer = Convert.ToString(item["Manufacturer"]);
            var pnpClass = Convert.ToString(item["PNPClass"]);
            var service = Convert.ToString(item["Service"]);

            if (!LooksKeyboardRelated(deviceId, name, pnpClass, service))
                continue;

            var vid = Match(VidRegex, deviceId);
            var pid = Match(PidRegex, deviceId);
            var haystack = $"{deviceId} {name} {manufacturer} {service}".ToUpperInvariant();

            yield return new KeyboardDeviceInfo(
                DeviceId: deviceId,
                Name: name,
                Manufacturer: manufacturer,
                PnpClass: pnpClass,
                Service: service,
                Vid: vid,
                Pid: pid,
                LooksLikeAppleKeyboard: haystack.Contains("VID_05AC")
                    || haystack.Contains("VID&0001004C")
                    || haystack.Contains("MAGIC KEYBOARD")
                    || haystack.Contains("妙控键盘"),
                LooksLikeBluetooth: haystack.Contains("BTH") || haystack.Contains("BLUETOOTH"),
                LooksLikeUsb: haystack.Contains("USB")
            );
        }
    }

    private static bool LooksKeyboardRelated(string deviceId, string name, string? pnpClass, string? service)
    {
        var haystack = $"{deviceId} {name} {pnpClass} {service}".ToUpperInvariant();
        return haystack.Contains("KEYBOARD")
               || haystack.Contains("HID")
               || haystack.Contains("MAGIC KEYBOARD")
               || haystack.Contains("妙控键盘")
               || haystack.Contains("BTHENUM");
    }

    private static string? Match(Regex regex, string value)
    {
        var match = regex.Match(value);
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }
}
