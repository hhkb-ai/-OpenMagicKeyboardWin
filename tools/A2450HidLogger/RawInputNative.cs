using System.Runtime.InteropServices;
using System.Text;

namespace A2450HidLogger;

internal static class RawInputNative
{
    public const int WM_INPUT = 0x00FF;
    public const int RID_INPUT = 0x10000003;
    public const int RIM_TYPEKEYBOARD = 1;
    public const int RIDEV_INPUTSINK = 0x00000100;
    public const int RIDI_DEVICENAME = 0x20000007;

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public int dwFlags;
        public IntPtr hwndTarget;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
        public int dwType;
        public int dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWKEYBOARD
    {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;
        public ushort VKey;
        public uint Message;
        public uint ExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUT
    {
        public RAWINPUTHEADER header;
        public RAWKEYBOARD keyboard;
    }

    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool RegisterRawInputDevices(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] RAWINPUTDEVICE[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern uint GetRawInputData(
        IntPtr hRawInput,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize,
        uint cbSizeHeader);

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint GetRawInputDeviceInfo(
        IntPtr hDevice,
        uint uiCommand,
        StringBuilder? pData,
        ref uint pcbSize);

    public static string? GetDeviceName(IntPtr hDevice)
    {
        if (hDevice == IntPtr.Zero)
            return null;

        uint size = 0;
        GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, null, ref size);
        if (size == 0)
            return null;

        var builder = new StringBuilder((int)size);
        var result = GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, builder, ref size);
        return result == uint.MaxValue ? null : builder.ToString();
    }
}
