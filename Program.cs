using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

class UniversalWindowInspector
{
    const uint WDA_NONE = 0x00000000;
    const uint WDA_MONITOR = 0x00000001;
    const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

    const int GWL_EXSTYLE = -20;
    const long WS_EX_APPWINDOW = 0x00040000L;
    const long WS_EX_TOOLWINDOW = 0x00000080L;
    const uint GW_OWNER = 4;

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetClassNameW(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowDisplayAffinity(IntPtr hWnd, out uint pdwAffinity);

    [DllImport("user32.dll")]
    static extern IntPtr GetWindowLongPtrW(IntPtr hWnd, int nIndex);

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Universal Window Flags Inspector";

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Universal Windows Window Inspector ===");
            Console.WriteLine("Hint: enter a process name, window title, class name, PID or HWND (0x...)");
            Console.Write("Filter (or Enter to list all active and protected windows): ");
            string filter = Console.ReadLine()?.Trim() ?? "";

            Console.WriteLine();
            int count = 0;

            uint targetPid = 0;
            bool isPidFilter = uint.TryParse(filter, out targetPid);

            long targetHwnd = 0;
            bool isHwndFilter = filter.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                                long.TryParse(filter.Substring(2), NumberStyles.HexNumber, null, out targetHwnd);

            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                string procName = GetProcessName(pid);

                var titleSb = new StringBuilder(512);
                GetWindowTextW(hWnd, titleSb, titleSb.Capacity);
                string title = titleSb.ToString();

                var classSb = new StringBuilder(256);
                GetClassNameW(hWnd, classSb, classSb.Capacity);
                string className = classSb.ToString();

                bool isVisible = IsWindowVisible(hWnd);
                IntPtr ownerHwnd = GetWindow(hWnd, GW_OWNER);
                bool hasOwner = ownerHwnd != IntPtr.Zero;

                bool affinityOk = GetWindowDisplayAffinity(hWnd, out uint affinity);
                int lastErr = Marshal.GetLastWin32Error();

                if (string.IsNullOrEmpty(filter))
                {
                    bool hasProtection = affinityOk && affinity != WDA_NONE;
                    if ((!isVisible || string.IsNullOrEmpty(title)) && !hasProtection)
                        return true;
                }
                else
                {
                    bool match =
                        (isPidFilter && pid == targetPid) ||
                        (isHwndFilter && hWnd.ToInt64() == targetHwnd) ||
                        procName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        className.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!match) return true;
                }

                long exStyle = GetWindowLongPtrW(hWnd, GWL_EXSTYLE).ToInt64();
                bool hasToolWindow = (exStyle & WS_EX_TOOLWINDOW) != 0;
                bool hasAppWindow = (exStyle & WS_EX_APPWINDOW) != 0;

                bool isHiddenFromTaskbar = !isVisible || ((hasToolWindow || hasOwner) && !hasAppWindow);

                string affinityText;
                if (!affinityOk)
                {
                    affinityText = lastErr == 5
                        ? "ACCESS DENIED (window belongs to an elevated process)"
                        : $"READ ERROR (Win32 Error: {lastErr})";
                }
                else
                {
                    affinityText = affinity switch
                    {
                        WDA_EXCLUDEFROMCAPTURE => "0x11 [WDA_EXCLUDEFROMCAPTURE - EXCLUDED FROM CAPTURE]",
                        WDA_MONITOR => "0x01 [WDA_MONITOR - BLACK SCREEN ON CAPTURE]",
                        WDA_NONE => "0x00 [WDA_NONE - NORMAL WINDOW]",
                        _ => $"0x{affinity:X2} [UNKNOWN FLAG]"
                    };
                }

                count++;
                Console.WriteLine($"[{count}] {procName}.exe (PID: {pid}) | HWND: 0x{hWnd.ToInt64():X}");
                Console.WriteLine($"    Title:   {(string.IsNullOrEmpty(title) ? "<no title>" : title)}");
                Console.WriteLine($"    Class:   {className}");
                Console.WriteLine($"    State:   Visible: {(isVisible ? "Yes" : "No")} | Owner: {(hasOwner ? $"0x{ownerHwnd.ToInt64():X}" : "None")}");
                Console.WriteLine($"    Capture: {affinityText}");
                Console.WriteLine($"    Taskbar: {(isHiddenFromTaskbar ? "HIDDEN from taskbar and Alt+Tab" : "VISIBLE")} (ExStyle: 0x{exStyle:X})");
                Console.WriteLine(new string('-', 68));

                return true;
            }, IntPtr.Zero);

            Console.WriteLine($"\nWindows found: {count}. Press Enter to rescan...");
            Console.ReadLine();
        }
    }

    static string GetProcessName(uint pid)
    {
        try
        {
            return Process.GetProcessById((int)pid).ProcessName;
        }
        catch
        {
            return "system";
        }
    }
}