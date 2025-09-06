namespace DesktopSwitcher.Bindings;

using System.DirectoryServices.ActiveDirectory;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

using static System.Windows.Forms.AxHost;
using System.Text;

static class WindowBindings
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        NoSize = 0x1,
        NoMove = 0x2,
        NoZOrder = 0x4,
        FrameChanged = 0x20,
        ShowWindow = 0x40,
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);

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

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, ShowWindowFlags swFlags);

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

    [Flags]
    public enum WindowStyleFlags : long
    {
        Border = 0x00800000L,
        Caption	= 0x00C00000L,
        Child = 0x40000000L,
        Childwindow	= 0x40000000L,
        Clipchildren = 0x02000000L,
        Clipsiblings = 0x04000000L,
        Disabled = 0x08000000L,
        Dlgframe = 0x00400000L,
        Group = 0x00020000L,
        Hscroll	= 0x00100000L,
        Iconic = 0x20000000L,
        Maximize = 0x01000000L,
        Maximizebox	= 0x00010000L,
        Minimize = 0x20000000L,
        Minimizebox	= 0x00020000L,
        Overlapped = 0x00000000L,
        Popup = 0x80000000L,
        Sizebox	= 0x00040000L,
        Sysmenu	= 0x00080000L,
        Tabstop	= 0x00010000L,
        Thickframe	= 0x00040000L,
        Tiled = 0x00000000L,
        Visible	= 0x10000000L,
        Vscroll = 0x00200000L,
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out CommonBindings.RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr GetTopWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowFlags uCmd);

    [Flags]
    public enum GetWindowFlags
    {
        WindowFirst = 0,
        WindowLast = 1,
        WindowNext = 2,
        WindowPrev = 3,
        Owner = 4,
        Child = 5,
        EnabledPopup = 6,
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags gaFlags);

    [Flags]
    public enum GetAncestorFlags : uint
    {
        Parent = 1,
        Root = 2,
        RootOwner = 3,
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetLastActivePopup(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool SystemParametersInfo(int uAction, int uParam, ref CommonBindings.RECT lpvParam, int fuWinIni);

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public CommonBindings.RECT rc;
        public int lParam;
    }

    [DllImport("shell32.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    public enum WinUserEvents
    {
        EventSystemForeground = 0x3,
        EventSystemExitSizeMove = 0x000B,
    }
}
