namespace DesktopSwitcher.Bindings;

using System.Runtime.InteropServices;

using static System.Windows.Forms.AxHost;

static class WindowBindings
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        NoSize = 0x1,
        NoZOrder = 0x4,
        ShowWindow = 0x40,
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public uint Length;
        public uint Flags;
        public uint ShowCmd;
        public CommonBindings.RECT PtMinPosition;
        public CommonBindings.POINT PtMaxPosition;
        public CommonBindings.RECT CcNormalPosition;
        public CommonBindings.RECT CcDevice;
    }

    [Flags]
    public enum ShowWindowFlags
    {
        Hide = 0,
        ShowNormal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        ShowMaximized = 3,
        ShowNoActive = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActive = 7,
        ShowNa = 8,
        Restore = 9,
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out CommonBindings.RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    public enum WinUserEvents
    {
        EventSystemForeground = 0x3,
    }
}
