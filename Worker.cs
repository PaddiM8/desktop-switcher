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
        var thread = new Thread(RegisterHotKeys);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        var mappings = new List<(Modifiers, Keys, Action)>
        {
            (Modifiers.Alt, Keys.D1, () => Switch(0)),
            (Modifiers.Alt, Keys.D2, () => Switch(1)),
            (Modifiers.Alt, Keys.D3, () => Switch(2)),
            (Modifiers.Alt, Keys.D4, () => Switch(3)),
            (Modifiers.Alt, Keys.D5, () => Switch(4)),
            (Modifiers.Alt, Keys.D6, () => Switch(5)),
            (Modifiers.Alt, Keys.D7, () => Switch(6)),
            (Modifiers.Alt, Keys.D8, () => Switch(7)),
            (Modifiers.Alt, Keys.D9, () => Switch(8)),
            (Modifiers.Alt | Modifiers.Control, Keys.D1, () => MoveTo(0)),
            (Modifiers.Alt | Modifiers.Control, Keys.D2, () => MoveTo(1)),
            (Modifiers.Alt | Modifiers.Control, Keys.D3, () => MoveTo(2)),
            (Modifiers.Alt | Modifiers.Control, Keys.D4, () => MoveTo(3)),
            (Modifiers.Alt | Modifiers.Control, Keys.D5, () => MoveTo(4)),
            (Modifiers.Alt | Modifiers.Control, Keys.D6, () => MoveTo(5)),
            (Modifiers.Alt | Modifiers.Control, Keys.D7, () => MoveTo(6)),
            (Modifiers.Alt | Modifiers.Control, Keys.D8, () => MoveTo(7)),
            (Modifiers.Alt | Modifiers.Control, Keys.D9, () => MoveTo(8)),
            (Modifiers.Alt | Modifiers.Shift, Keys.W, () => MoveTo(8)),
        };

        foreach (var (modifiers, keys, handler) in mappings)
        {
            var id = _hotKeyManager.RegisterHotKey(modifiers, keys, handler);
            if (id.HasValue)
                _hotKeyIds.Add(id.Value);
        }

        Application.Run();

        return Task.CompletedTask;
    }

    private static void Switch(int index)
    {
        VirtualDesktop.GetDesktops().ElementAtOrDefault(index)?.Switch();
    }

    private static void MoveTo(int index)
    {
        var desktop = VirtualDesktop.GetDesktops().ElementAtOrDefault(index);
        if (desktop == null)
            return;

        VirtualDesktop.MoveToDesktop(Bindings.GetForegroundWindow(), desktop);
    }

    private void RegisterHotKeys()
    {
        var desktopCount = VirtualDesktop.GetDesktops().Length;
        if (desktopCount < 10)
        {
            for (var i = 0; i < 10 - desktopCount; i++)
                VirtualDesktop.Create();
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var id in _hotKeyIds)
            _hotKeyManager.UnregisterHotKey(id);

        return Task.CompletedTask;
    }
}