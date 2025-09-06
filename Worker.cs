using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using WindowsDesktop;

namespace DesktopSwitcher;

public class Worker : BackgroundService
{
    private readonly HotKeyManager _hotKeyManager = new();
    private readonly List<int> _hotKeyIds = new();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var windowManager = new WindowManager();
        var workspaceManager = new WorkspaceManager(windowManager);

        workspaceManager.MoveWorkspaceToMonitor(3, GetSecondaryMonitor2());
        workspaceManager.MoveWorkspaceToMonitor(4, GetSecondaryMonitor1());

        var mappings = new List<(Modifiers, Keys, Action)>
        {
            (Modifiers.Alt, Keys.D1, () => workspaceManager.SwitchToWorkspace(5)),
            (Modifiers.Alt, Keys.D2, () => workspaceManager.SwitchToWorkspace(6)),
            (Modifiers.Alt, Keys.D3, () => workspaceManager.SwitchToWorkspace(7)),
            (Modifiers.Alt, Keys.D4, () => workspaceManager.SwitchToWorkspace(8)),
            (Modifiers.Alt, Keys.D5, () => workspaceManager.SwitchToWorkspace(9)),
            (Modifiers.Alt, Keys.D6, () => workspaceManager.SwitchToWorkspace(0)),
            (Modifiers.Alt, Keys.D7, () => workspaceManager.SwitchToWorkspace(1)),
            (Modifiers.Alt, Keys.D8, () => workspaceManager.SwitchToWorkspace(2)),
            (Modifiers.Alt, Keys.D9, () => workspaceManager.SwitchToWorkspace(3)),
            (Modifiers.Alt, Keys.D0, () => workspaceManager.SwitchToWorkspace(4)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D1, () => workspaceManager.MoveWindowToWorkspace(5)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D2, () => workspaceManager.MoveWindowToWorkspace(6)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D3, () => workspaceManager.MoveWindowToWorkspace(7)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D4, () => workspaceManager.MoveWindowToWorkspace(8)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D5, () => workspaceManager.MoveWindowToWorkspace(9)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D6, () => workspaceManager.MoveWindowToWorkspace(0)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D7, () => workspaceManager.MoveWindowToWorkspace(1)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D8, () => workspaceManager.MoveWindowToWorkspace(2)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D9, () => workspaceManager.MoveWindowToWorkspace(3)),
            (Modifiers.Alt | Modifiers.Shift, Keys.D0, () => workspaceManager.MoveWindowToWorkspace(4)),
            (Modifiers.Alt | Modifiers.Shift, Keys.Q, () => WindowManager.CloseWindow()),
            (Modifiers.Alt | Modifiers.Shift, Keys.W, () => WorkspaceManager.TogglePinned()),
            (Modifiers.Alt, Keys.F, () => WindowManager.ToggleFullscreen()),
        };

        foreach (var (modifiers, keys, handler) in mappings)
        {
            var id = _hotKeyManager.RegisterHotKey(modifiers, keys, handler);
            if (id.HasValue)
                _hotKeyIds.Add(id.Value);
        }

        VirtualDesktop.PinApplication("MSTeams_8wekyb3d8bbwe!MSTeams");
        VirtualDesktop.PinApplication("Microsoft.OutlookForWindows_8wekyb3d8bbwe!Microsoft.OutlookforWindows");

        Application.Run();

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var id in _hotKeyIds)
            _hotKeyManager.UnregisterHotKey(id);

        return Task.CompletedTask;
    }

    private MonitorInfo GetSecondaryMonitor1()
    {
        var monitors = WindowManager.GetMonitors();

        return monitors.FirstOrDefault(x => x.Kind == MonitorKind.Right)
            ?? monitors.FirstOrDefault(x => x.Kind == MonitorKind.Bottom)
            ?? monitors.FirstOrDefault(x => x.Kind == MonitorKind.Primary)
            ?? monitors.First();
    }

    private MonitorInfo GetSecondaryMonitor2()
    {
        var monitors = WindowManager.GetMonitors();

        return monitors.FirstOrDefault(x => x.Kind == MonitorKind.Left)
            ?? monitors.FirstOrDefault(x => x.Kind == MonitorKind.Primary)
            ?? monitors.First();
    }
}