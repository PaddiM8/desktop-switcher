namespace DesktopSwitcher.Bindings;

using System;
using System.Runtime.InteropServices;

static class HotKeyBindings
{
    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
