# Codex Prompts

## Phase 1: A2450 HID Logger

```text
You are a Windows HID / Raw Input / C# engineer.

Continue the OpenMagicKeyboardWin project. The first target device is Apple Magic Keyboard A2450.

Improve tools/A2450HidLogger so it can safely collect non-textual Raw Input and HID metadata needed to implement Fn/Ctrl behavior.

Requirements:
- Do not log typed text.
- Log only device path, virtual key, scan code, flags, make/break state, and raw report bytes when available.
- Improve Apple Magic Keyboard A2450 detection.
- Add an explicit test workflow for Fn, Left Ctrl, F-row, Backspace, arrows, Command, Option, and Lock/Eject.
- Export a final logs/a2450-hid-report.json summary.
```

## Phase 2: Driver design

```text
You are a Windows WDK / KMDF / HIDClass lower filter driver engineer.

Design the OpenMagicKeyboardA2450Filter driver for OpenMagicKeyboardWin.

Core behavior:
- Physical Fn emits Left Ctrl.
- Physical Left Ctrl becomes internal FnLayer.
- FnLayer + Backspace emits Delete.
- FnLayer + arrows emit Home/End/PageUp/PageDown.
- Only target Apple Magic Keyboard A2450.

Do not copy proprietary software or GPL code. Use clean-room implementation.
```
