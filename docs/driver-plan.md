# A2450 Driver Plan

## Core goal

Implement Fn and Left Ctrl behavior for Apple Magic Keyboard A2450 on Windows 10/11.

## Important design point

Windows does not provide a universal `VK_FN` like `VK_CONTROL`.

Therefore the practical swap design is:

- Original Fn → emit Left Ctrl
- Original Left Ctrl → internal FnLayer state

FnLayer is then translated inside the driver:

- FnLayer + Backspace → Delete
- FnLayer + Up → Page Up
- FnLayer + Down → Page Down
- FnLayer + Left → Home
- FnLayer + Right → End
- FnLayer + F-row → media/function layer behavior

## Driver approach candidates

### Option A: HIDClass lower filter

Pros:

- Closest to existing Apple keyboard filter projects.
- Can target a specific HID device.
- Can operate below ordinary keyboard remappers.

Cons:

- Requires WDK knowledge.
- Requires test signing for development.
- Requires formal driver signing for public distribution.

### Option B: Keyboard class filter

Pros:

- Familiar pattern through Microsoft's Kbfiltr sample.

Cons:

- Harder to avoid affecting other keyboards unless carefully scoped.
- May not expose Apple vendor-defined Fn information.

## MVP

- A2450 only.
- Bluetooth only first.
- Fn → Left Ctrl.
- Left Ctrl → internal FnLayer.
- No tray app dependency for the first prototype.
