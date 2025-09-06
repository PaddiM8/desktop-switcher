namespace DesktopSwitcher;

using WindowsDesktop;

class WorkspaceManager
{
    private static bool _hasInitialised;

    private readonly WindowManager _windowManager;
    private readonly Dictionary<int, MonitorInfo> _workspaceByMonitor = new();

    public WorkspaceManager(WindowManager windowManager)
    {
        if (!_hasInitialised)
        {
            Init();
            _hasInitialised = true;
        }

        _windowManager = windowManager;

        var primaryMonitor = WindowManager.GetPrimaryMonitor();
        foreach (var (workspace, index) in VirtualDesktop.GetDesktops().Select((x, i) => (x, i)))
            _workspaceByMonitor[index] = primaryMonitor;
    }

    private static void Init()
    {
        var thread = new Thread(InitialiseWorkspaces);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    public static void TogglePinned()
    {
        var window = WindowManager.GetFocusedWindow();
        if (window == IntPtr.Zero)
            return;

        if (VirtualDesktop.IsPinnedWindow(window))
        {
            VirtualDesktop.UnpinWindow(window);
        }
        else
        {
            VirtualDesktop.PinWindow(window);
        }
    }

    public void SwitchToWorkspace(int index)
    {
        VirtualDesktop.GetDesktops().ElementAtOrDefault(index)?.Switch();

        if (_workspaceByMonitor.TryGetValue(index, out var monitor))
        {
            _windowManager.FocusMonitor(monitor);

            if (monitor.Kind != MonitorKind.Primary)
                return;
        }
    }

    public void MoveWindowToWorkspace(int index)
    {
        var window = WindowManager.GetFocusedWindow();
        if (_workspaceByMonitor.TryGetValue(index, out var monitor))
        {
            _windowManager.FocusMonitor(monitor);

            if (monitor.Kind != MonitorKind.Primary)
            {
                _windowManager.MoveWindowToMonitor(window, monitor);
                VirtualDesktop.PinWindow(window);
                return;
            }
        }

        var desktop = VirtualDesktop.GetDesktops().ElementAtOrDefault(index);
        if (desktop == null)
            return;

        if (VirtualDesktop.IsPinnedWindow(window))
            VirtualDesktop.UnpinWindow(window);

        VirtualDesktop.MoveToDesktop(window, desktop);
    }

    public void MoveWorkspaceToMonitor(int index, MonitorInfo monitor)
    {
        _workspaceByMonitor[index] = monitor;
    }

    private static void InitialiseWorkspaces()
    {
        var desktopCount = VirtualDesktop.GetDesktops().Length;
        if (desktopCount < 10)
        {
            for (var i = 0; i < 10 - desktopCount; i++)
                VirtualDesktop.Create();
        }
    }
}
