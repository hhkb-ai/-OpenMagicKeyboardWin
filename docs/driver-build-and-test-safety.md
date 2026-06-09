# Driver Build and Test Safety

## Current Stage

**Build scaffold only. No installation.**

The driver source code and WDK project file exist as a design artifact. The `.sys` file produced by building is NOT meant to be loaded on any system.

## Build Is Not Installation

Compiling the driver in Visual Studio produces a `.sys` file in the output directory. This file is inert — it does nothing until:

1. An INF file references it
2. Windows loads it via the PnP manager
3. It binds to a hardware device

Simply building the project does NOT install the driver, modify the registry, or affect the running system.

## Prohibited Actions

The following actions are **strictly forbidden** during the current development stage:

| Action | Why |
|--------|-----|
| `bcdedit /set testsigning on` | Modifies boot configuration, enables unsigned driver loading |
| Disabling Secure Boot | Reduces system security |
| `pnputil /add-driver ...` | Installs the driver into the driver store |
| `devcon install ...` | Installs and starts the driver |
| Device Manager "Update Driver" | Installs the driver via GUI |
| Modifying `HKLM\SYSTEM\CurrentControlSet\Control\Class\{...}\LowerFilters` | Registers the driver as a filter |
| Binding to A2450 hardware | Would intercept keyboard input |
| Sending Output Reports to COL02 | Could interfere with device state |

## Safe Actions

These actions are safe and expected during development:

| Action | Notes |
|--------|-------|
| Building in Visual Studio + WDK | Produces .sys only, no side effects |
| Running C# unit tests | Tests transform logic in user mode |
| Reviewing generated .sys | Static analysis of the binary |
| Running DescriptorDump | Read-only device enumeration |
| Running ConsumerControlProbe | Read-only device enumeration |
| Static code review | Reading source files |

## Future Test Requirements

Before the driver can be installed (in a future phase), ALL of the following must be in place:

### Environment

- [ ] Test machine or VM (NOT a production machine)
- [ ] System restore point created
- [ ] Full system backup
- [ ] Spare keyboard and mouse available (USB, not Bluetooth)
- [ ] Windows Recovery Environment accessible

### Driver Signing

- [ ] EV code signing certificate obtained
- [ ] Microsoft Hardware Dashboard account set up
- [ ] Driver submission and attestation signing completed
- OR
- [ ] TESTSIGNING mode explicitly accepted by the user
- [ ] Secure Boot implications documented

### Safety Measures

- [ ] Driver unload script ready
- [ ] Safe mode boot instructions documented
- [ ] Rollback plan tested
- [ ] BSOD recovery plan documented

### Testing Checklist

- [ ] Fn → Left Ctrl works
- [ ] Left Ctrl → FnLayer works
- [ ] FnLayer + Backspace → Delete works
- [ ] FnLayer + arrows → Home/End/PageUp/PageDown works
- [ ] Normal typing unaffected
- [ ] Other keyboards unaffected
- [ ] No BSOD over 24h period
- [ ] Driver unloads cleanly

## Recovery Plan Placeholder

If the driver causes issues after installation:

1. **Keyboard stops working**: Use spare USB keyboard
2. **System won't boot**: Boot into Safe Mode (F8 / Shift+Restart)
3. **In Safe Mode**: Open Device Manager, uninstall the filter driver
4. **If Safe Mode fails**: Use Windows Recovery → Command Prompt → `sc delete OpenMagicKeyboardA2450Filter`
5. **Nuclear option**: System Restore to the pre-installation restore point

**This recovery plan is a placeholder. Do not test it now.**
