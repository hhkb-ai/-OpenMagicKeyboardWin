using System.Text.Json;

namespace A2450HidLogger;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true
    };

    public static readonly JsonSerializerOptions Compact = new()
    {
        WriteIndented = false
    };
}
