# Security Policy

OpenMagicKeyboardWin handles keyboard input at a low level. The project must be designed to avoid becoming a keylogger.

## Rules

- Do not record typed text.
- Do not upload logs automatically.
- Do not collect active window titles.
- Do not collect clipboard data.
- Log only device metadata, virtual key codes, scan codes, make/break flags, and raw report bytes needed for device research.

## Reporting security issues

Open a private security advisory on GitHub when available, or create a minimal issue that does not disclose exploit details.
