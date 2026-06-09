# A2450 HID Test Plan

Target device: Apple Magic Keyboard A2450.

Priority connection method: Bluetooth.

## Data to collect

- Windows version
- Keyboard model printed on device
- Device name shown in Windows Bluetooth settings
- Device Manager hardware IDs
- VID / PID
- Device path
- Product string
- Manufacturer string
- Raw Input events
- HID report bytes, if available

## Key matrix

Test each item as press and release:

- Fn / Globe
- Left Ctrl
- Right Ctrl
- Fn + Left Ctrl
- Command
- Option
- F1-F12
- Fn + F1-F12
- Backspace
- Fn + Backspace
- Arrow Up / Down / Left / Right
- Fn + Arrow Up / Down / Left / Right
- Lock / Eject key, if present

## Expected research result

We need to determine whether A2450 exposes Fn as:

1. A standard keyboard event.
2. A vendor-defined HID usage.
3. Only a hardware-layer modifier that changes reports.
4. Not visible in user mode.

This determines whether Fn/Ctrl swap can be implemented in user mode or must be handled in a keyboard/HID filter driver.
