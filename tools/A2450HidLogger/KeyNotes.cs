namespace A2450HidLogger;

internal static class KeyNotes
{
    public static string? Describe(int virtualKey, int scanCode, int flags)
    {
        return virtualKey switch
        {
            0xA2 => "VK_LCONTROL",
            0xA3 => "VK_RCONTROL",
            0x5B => "VK_LWIN / Command candidate",
            0x5C => "VK_RWIN / Command candidate",
            0xA4 => "VK_LMENU / Option candidate",
            0xA5 => "VK_RMENU / Option candidate",
            0x70 => "F1",
            0x71 => "F2",
            0x72 => "F3",
            0x73 => "F4",
            0x74 => "F5",
            0x75 => "F6",
            0x76 => "F7",
            0x77 => "F8",
            0x78 => "F9",
            0x79 => "F10",
            0x7A => "F11",
            0x7B => "F12",
            0x08 => "Backspace",
            0x25 => "Left Arrow",
            0x26 => "Up Arrow",
            0x27 => "Right Arrow",
            0x28 => "Down Arrow",
            _ => null
        };
    }
}
