namespace DesktopSwitcher;

using DesktopSwitcher.Bindings;

using System.Runtime.InteropServices;

internal class WindowManager
{
    private const uint MONITOR_DEFAULTTONEAREST = 0x2;
    private const int MONITORINFOF_PRIMARY = 0x1;

    private static WindowBindings.WinEventDelegate _procDelegate = new(WinEventProc);
    private static Dictionary<IntPtr, IntPtr> _monitorLastWindow = new();

    static WindowManager()
    {
        MonitorBindings.SetProcessDpiAwarenessContext(-3);
    }

    public WindowManager()
    {
        // Window focus changed event
        WindowBindings.SetWinEventHook(
            (uint)WindowBindings.WinUserEvents.EventSystemForeground,
            (uint)WindowBindings.WinUserEvents.EventSystemForeground,
            IntPtr.Zero,
            _procDelegate,
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
            return;

        if (_monitorLastWindow.TryGetValue(monitor.Handle, out var window))
        {
            WindowBindings.SetForegroundWindow(window);
        }

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

    private static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (hwnd == IntPtr.Zero)
            return;

        var monitor = MonitorBindings.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        _monitorLastWindow[monitor] = hwnd;
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
