# 🕵️‍♂️ Universal Window Flags Inspector (Professional Window Snitch)

A tiny C# console snitch that interrogates Windows about every visible window on your screen. Built specifically to check if your `0x11` anti-capture cloak actually works or if you're accidentally leaking your screen to Discord.

## 🔍 What this snitch exposes
* **Process & PID:** Who birthed this window (e.g., `javaw.exe`).
* **HWND:** The window's government ID number.
* **Title & Class:** Its legal name and species (modern Minecraft is a `GLFW30`).
* **Capture Status:**
  * `0x00` — Naked window (OBS and screenshots see everything).
  * `0x11` — Stealth mode active (pure void on recordings).
* **Taskbar:** Tells you if the window is still chilling on your taskbar / `Alt+Tab` or successfully banished to the shadow realm.

## 🎮 How to use
1. Run the `.exe`.
3. Type part of a process name or window title (like `javaw` or `chrome`) and hit **Enter**.
4. Or just smash **Enter** with an empty box to dox *every* visible window on your PC.
5. Hit **Enter** again whenever you want to refresh the snitch report.

## 🤷‍♂️ Why it's cool
Zero admin rights needed. It doesn't touch or poke other apps' memory—it just politely asks `user32.dll` for the gossip. Great for making sure Windows didn't reset your `0x11` flag after you alt-tabbed too aggressively.
