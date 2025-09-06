namespace DesktopSwitcher.Bindings;

using System.Runtime.InteropServices;

static class CursorBindings
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
}
