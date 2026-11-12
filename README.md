# Universal Window Inspector

A small console utility for Windows that inspects the windows of every
running process in real time and reports their extended flags: visibility,
owner window, `WS_EX_TOOLWINDOW` / `WS_EX_APPWINDOW` styles, and — most
importantly — the **display affinity** (`SetWindowDisplayAffinity`).

The tool is especially useful for spotting windows that:

- are hidden from the taskbar and Alt+Tab;
- are excluded from screen capture (`WDA_EXCLUDEFROMCAPTURE`);
- render as a black rectangle when captured (`WDA_MONITOR`);
- belong to elevated processes (reported as `Access Denied` when reading
  the affinity).

## Features

- Filter by **process name**, **window title**, **window class**, **PID**, or
  **HWND** (hexadecimal, e.g. `0x1A2B3C`).
- An empty filter lists every active and protected window.
- Determines taskbar hiding via the combination of `WS_EX_TOOLWINDOW`,
  `WS_EX_APPWINDOW`, and the presence of an owner window.
- Human-readable capture protection state:
  - `WDA_NONE` — ordinary window;
  - `WDA_MONITOR` — renders as a black screen when captured;
  - `WDA_EXCLUDEFROMCAPTURE` — fully excluded from capture;
  - `Access Denied` — the window belongs to a process with higher integrity.
- Cyclic scanning: press Enter after inspecting the results to refresh the list.

## Usage

1. Run `WindowFlagChecker.exe` (recommended: as Administrator, so that
   affinity of elevated processes is readable).
2. Type a filter or leave the line empty and press Enter.
3. Inspect the results. Press Enter again to rescan.
