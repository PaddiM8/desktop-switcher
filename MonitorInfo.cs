namespace DesktopSwitcher;

using DesktopSwitcher.Bindings;

class MonitorInfo
{
    public IntPtr Handle { get; init; }

    public CommonBindings.RECT Bounds { get; init; }

    public MonitorKind Kind { get; set;  }
}
