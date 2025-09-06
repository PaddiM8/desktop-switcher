namespace DesktopSwitcher;

using DesktopSwitcher.Bindings;

using System.Runtime.InteropServices;
using System.Text;

using WindowsDesktop;

internal class WindowManager
{
    private const uint MONITOR_DEFAULTTONULL = 0x0;
    private const uint MONITOR_DEFAULTTONEAREST = 0x2;
    private const int MONITORINFOF_PRIMARY = 0x1;

    private static Dictionary<IntPtr, IntPtr> _monitorLastWindow = [];
    private static Dictionary<VirtualDesktop, IntPtr> _workspaceLastWindow = [];
    private readonly WindowBindings.WinEventDelegate _onWindowFocused;
    private readonly WindowBindings.WinEventDelegate _onWindowMovedOrResized;

    static WindowManager()
    {
        MonitorBindings.SetProcessDpiAwarenessContext(-3);
    }

    public WindowManager()
    {
        // Window focus changed event
        _onWindowFocused = new WindowBindings.WinEventDelegate(OnWindowFocused);
        WindowBindings.SetWinEventHook(
            (uint)WindowBindings.WinUserEvents.EventSystemForeground,
            (uint)WindowBindings.WinUserEvents.EventSystemForeground,
            IntPtr.Zero,
            _onWindowFocused,
            0,
            0,
            0
        );

        // Window move/resize finished event
        _onWindowMovedOrResized = new WindowBindings.WinEventDelegate(OnWindowMovedOrResized);
        WindowBindings.SetWinEventHook(
            (uint)WindowBindings.WinUserEvents.EventSystemExitSizeMove,
            (uint)WindowBindings.WinUserEvents.EventSystemExitSizeMove,
            IntPtr.Zero,
            _onWindowMovedOrResized,
            0,
            0,
            0
        );
    }

    public static void ToggleFullscreen()
    {
        var window = GetFocusedWindow();
        if (window == IntPtr.Zero)
            return;

        var windowPlacement = new WindowBindings.WINDOWPLACEMENT();
        if (!WindowBindings.GetWindowPlacement(window, ref windowPlacement))
            return;

        if (windowPlacement.ShowCmd == (uint)WindowBindings.ShowWindowFlags.ShowMaximized)
        {
            windowPlacement.ShowCmd = (uint)WindowBindings.ShowWindowFlags.ShowNormal;
        }
        else
        {
            windowPlacement.ShowCmd = (uint)WindowBindings.ShowWindowFlags.ShowMaximized;
        }

        WindowBindings.SetWindowPlacement(window, ref windowPlacement);
    }

    public static IntPtr GetFocusedWindow()
    {
        return WindowBindings.GetForegroundWindow();
    }

    public static void CloseWindow()
    {
        var window = GetFocusedWindow();
        if (window == IntPtr.Zero)
            return;

        WindowBindings.SendMessage(window, 0x10, IntPtr.Zero, IntPtr.Zero);
    }

    public static void SetWindowPosition(IntPtr windowHandle, int x, int y)
    {
        var flags = (uint)(WindowBindings.SetWindowPosFlags.NoSize | WindowBindings.SetWindowPosFlags.NoZOrder | WindowBindings.SetWindowPosFlags.ShowWindow);
        WindowBindings.SetWindowPos(windowHandle, IntPtr.Zero, x, y, 0, 0, flags);
    }

    public void FocusMonitor(MonitorKind kind)
    {
        var monitor = GetMonitors().FirstOrDefault(x => x.Kind == kind);
        if (monitor != null)
            FocusMonitor(monitor);
    }

    public void FocusMonitor(MonitorInfo monitor)
    {
        var currentMonitorHandle = MonitorBindings.MonitorFromPoint(new CommonBindings.POINT(Cursor.Position.X, Cursor.Position.Y), MONITOR_DEFAULTTONEAREST);
        if (monitor.Handle == currentMonitorHandle)
        {
            if (_workspaceLastWindow.TryGetValue(VirtualDesktop.Current, out var workspaceWindow))
                WindowBindings.SetForegroundWindow(workspaceWindow);

            return;
        }

        if (_monitorLastWindow.TryGetValue(monitor.Handle, out var window))
            WindowBindings.SetForegroundWindow(window);

        var (centerX, centerY) = GetMonitorCenter(monitor);
        CursorBindings.SetCursorPos(centerX, centerY);
    }

    public void MoveWindowToMonitor(IntPtr window, MonitorInfo monitor)
    {
        if (window == IntPtr.Zero)
            return;

        var monitorCenter = GetMonitorCenter(monitor);
        WindowBindings.GetWindowRect(window, out var windowBounds);
        SetWindowPosition(
            window,
            monitorCenter.x - windowBounds.Width / 2,
            monitorCenter.y - windowBounds.Height / 2
        );
    }

    public static MonitorInfo GetPrimaryMonitor()
    {
        var monitors = GetMonitors();

        return monitors.FirstOrDefault(x => x.Kind == MonitorKind.Primary)
            ?? monitors.First();
    }

    public static IntPtr GetMonitorHandle(IntPtr? windowHandle = null)
    {
        return MonitorBindings.MonitorFromWindow(
            windowHandle ?? GetFocusedWindow(),
            MONITOR_DEFAULTTONEAREST
        );
    }

    public static void FocusNextWindow()
    {
        var originalWindow = GetFocusedWindow();
        var monitorHandle = GetMonitorHandle(originalWindow);
        if (monitorHandle == IntPtr.Zero)
            return;

        IntPtr nextWindow = originalWindow;
        while (nextWindow != IntPtr.Zero)
        {
            var currentWindow = nextWindow;
            nextWindow = WindowBindings.GetWindow(nextWindow, (uint)WindowBindings.GetWindowFlags.WindowNext);
            if (nextWindow == IntPtr.Zero)
                nextWindow = WindowBindings.GetWindow(currentWindow, (uint)WindowBindings.GetWindowFlags.WindowFirst);

            if (!IsRealWindow(nextWindow))
                continue;

            if (VirtualDesktop.FromHwnd(nextWindow) == VirtualDesktop.Current)
                break;
        }

        if (nextWindow == IntPtr.Zero)
            return;

        WindowBindings.SetForegroundWindow(nextWindow);
    }

    public static void FocusPreviousWindow()
    {
        var originalWindow = GetFocusedWindow();
        var monitorHandle = GetMonitorHandle(originalWindow);
        if (monitorHandle == IntPtr.Zero)
            return;

        IntPtr previousWindow = originalWindow;
        while (previousWindow != IntPtr.Zero)
        {
            var currentWindow = previousWindow;
            previousWindow = WindowBindings.GetWindow(previousWindow, (uint)WindowBindings.GetWindowFlags.WindowPrev);
            if (previousWindow == IntPtr.Zero)
                previousWindow = WindowBindings.GetWindow(currentWindow, (uint)WindowBindings.GetWindowFlags.WindowLast);

            if (!IsRealWindow(previousWindow))
                continue;

            if (VirtualDesktop.FromHwnd(previousWindow) == VirtualDesktop.Current)
                break;
        }

        if (previousWindow == IntPtr.Zero)
            return;

        WindowBindings.SetForegroundWindow(previousWindow);
    }

    public static List<MonitorInfo> GetMonitors()
    {
        var infoPairs = new List<(MonitorInfo info, bool isPrimary)>();
        MonitorBindings.EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdc, ref CommonBindings.RECT _, IntPtr data) =>
            {
                var info = new MonitorBindings.MONITORINFOEX();
                info.cbSize = Marshal.SizeOf(info);
                MonitorBindings.GetMonitorInfo(hMonitor, ref info);

                var monitorInfo = new MonitorInfo
                {
                    Handle = hMonitor,
                    Bounds = info.rcMonitor,
                };
                var isPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;
                infoPairs.Add((monitorInfo, isPrimary));

                return true;
            },
            IntPtr.Zero
        );

        var primaryMonitor = infoPairs.FirstOrDefault(x => x.isPrimary).info;
        if (primaryMonitor == null)
        {
            return infoPairs
                .Select(x => x.info)
                .ToList();
        }

        primaryMonitor.Kind = MonitorKind.Primary;

        foreach (var (monitor, isPrimary) in infoPairs)
        {
            if (isPrimary)
                continue;

            if (monitor.Bounds.Right <= primaryMonitor.Bounds.Left)
            {
                monitor.Kind = MonitorKind.Left;
            }
            else if (monitor.Bounds.Bottom <= primaryMonitor.Bounds.Top)
            {
                monitor.Kind = MonitorKind.Top;
            }
            else if (monitor.Bounds.Left >= primaryMonitor.Bounds.Right)
            {
                monitor.Kind = MonitorKind.Right;
            }
            else if (monitor.Bounds.Top >= primaryMonitor.Bounds.Bottom)
            {
                monitor.Kind = MonitorKind.Bottom;
            }
        }

        return infoPairs
            .Select(x => x.info)
            .ToList();
    }

    public static List<IntPtr> GetWindowsInWorkspace()
    {
        var windows = new List<IntPtr>();
        WindowBindings.EnumDesktopWindows(
            IntPtr.Zero,
            (windowHandle, lParam) =>
            {
                if (!IsRealWindow(windowHandle))
                    return true;

                if (VirtualDesktop.FromHwnd(windowHandle) != VirtualDesktop.Current)
                    return true;

                windows.Add(windowHandle);

                return true;
            },
            IntPtr.Zero
        );

        var list = new List<string>();
        foreach (var window in windows)
        {
            var builder = new StringBuilder(256);
            WindowBindings.GetWindowText(window, builder, builder.Capacity);
            list.Add(builder.ToString());
        }

        return windows;
    }

    private static bool IsRealWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            return false;

        if (!WindowBindings.IsWindowVisible(windowHandle))
            return false;

        const int GWL_EXSTYLE = -20;
        const uint WS_EX_TOOLWINDOW = 0x00000080;
        const uint WS_EX_APPWINDOW = 0x00040000;
        uint exStyle = WindowBindings.GetWindowLong(windowHandle, GWL_EXSTYLE);

        if ((exStyle & WS_EX_TOOLWINDOW) != 0)
            return false;

        if ((exStyle & WS_EX_APPWINDOW) != 0)
            return true;

        int length = WindowBindings.GetWindowTextLength(windowHandle);
        if (length == 0)
            return false;

        return true;
    }

    private static void OnWindowFocused(IntPtr hWinEventHook, uint eventType, IntPtr windowHandle, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (windowHandle == IntPtr.Zero)
            return;

        var monitor = MonitorBindings.MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
        _monitorLastWindow[monitor] = windowHandle;
        _workspaceLastWindow[VirtualDesktop.Current] = windowHandle;

        PinWindowIfNotOnPrimaryMonitor(windowHandle);
    }

    private static void OnWindowMovedOrResized(IntPtr hWinEventHook, uint eventType, IntPtr windowHandle, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        PinWindowIfNotOnPrimaryMonitor(windowHandle);
    }

    private static void PinWindowIfNotOnPrimaryMonitor(IntPtr windowHandle)
    {
        if (!IsRealWindow(windowHandle))
            return;

        var monitor = MonitorBindings.MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
        if (monitor != GetPrimaryMonitor()?.Handle)
        {
            VirtualDesktop.PinWindow(windowHandle);
        }
        else if (VirtualDesktop.IsPinnedWindow(windowHandle))
        {
            VirtualDesktop.UnpinWindow(windowHandle);
        }
    }

    private static (int x, int y) GetMonitorCenter(MonitorInfo monitor)
    {
        var bounds = GetPhysicalRect(monitor.Bounds);
        var centerX = bounds.Left + bounds.Width / 2;
        var centerY = bounds.Top + bounds.Height / 2;

        return (centerX, centerY);
    }

    private static int GetPhysicalPixels(int pixels, uint dpi)
    {
        return (int)(pixels / (dpi / 96.0));
    }

    private static CommonBindings.RECT GetPhysicalRect(CommonBindings.RECT rect)
    {
        var currentMonitorHandle = MonitorBindings.MonitorFromPoint(new CommonBindings.POINT(Cursor.Position.X, Cursor.Position.Y), MONITOR_DEFAULTTONEAREST);
        MonitorBindings.GetDpiForMonitor(currentMonitorHandle, MonitorBindings.MonitorDpiType.Angular, out uint dpiX, out uint dpiY);

        return new CommonBindings.RECT(
            GetPhysicalPixels(rect.Left, dpiX),
            GetPhysicalPixels(rect.Top, dpiY),
            GetPhysicalPixels(rect.Right, dpiX),
            GetPhysicalPixels(rect.Bottom, dpiY)
        );
    }
}
