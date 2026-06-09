namespace OpenMagicKeyboard.Shared;

public sealed record KeyboardDeviceInfo(
    string DeviceId,
    string Name,
    string? Manufacturer,
    string? PnpClass,
    string? Service,
    string? Vid,
    string? Pid,
    bool LooksLikeAppleKeyboard,
    bool LooksLikeBluetooth,
    bool LooksLikeUsb
);
