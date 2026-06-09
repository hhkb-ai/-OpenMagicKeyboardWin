namespace OpenMagicKeyboard.Shared;

/// <summary>
/// HID Consumer Control Usage codes (Usage Page 0x0C).
/// These should be synthesized as Consumer Control Input Reports (not Output Reports).
/// COL02 is the Consumer Control Input Report channel (UsagePage 0x000C).
/// </summary>
public enum A2450ConsumerUsage : ushort
{
    ScanNextTrack = 0x00B5,
    ScanPreviousTrack = 0x00B6,
    Stop = 0x00B7,
    PlayPause = 0x00CD,
    Mute = 0x00E2,
    VolumeIncrement = 0x00E9,
    VolumeDecrement = 0x00EA,
}

/// <summary>
/// Maps FnLayer + F7~F12 to Consumer Control Usage codes.
///
/// Trigger condition: Physical Left Ctrl (FnLayer active) + F7~F12.
/// NOT Physical Fn + F7~F12, because Fn is mapped to Left Ctrl, not FnLayer.
///
/// Consumer Control reports are synthesized as Input Reports for COL02 (UsagePage 0x000C),
/// not via the COL01 keyboard report. Do not write Output Reports to the physical COL02 device.
/// </summary>
public static class A2450MediaKeyMapper
{
    /// <summary>
    /// Maps a HID key usage code to a Consumer Control usage, if it's an F7-F12 key.
    /// Returns null for non-media keys.
    /// </summary>
    public static ushort? MapFnLayerFunctionKeyToConsumerUsage(byte keyUsage)
    {
        return keyUsage switch
        {
            0x40 => (ushort)A2450ConsumerUsage.ScanPreviousTrack, // F7
            0x41 => (ushort)A2450ConsumerUsage.PlayPause,         // F8
            0x42 => (ushort)A2450ConsumerUsage.ScanNextTrack,     // F9
            0x43 => (ushort)A2450ConsumerUsage.Mute,              // F10
            0x44 => (ushort)A2450ConsumerUsage.VolumeDecrement,   // F11
            0x45 => (ushort)A2450ConsumerUsage.VolumeIncrement,   // F12
            _ => null,
        };
    }
}
