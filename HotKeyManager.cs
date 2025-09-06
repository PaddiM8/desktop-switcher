using DesktopSwitcher.Bindings;

using System.Runtime.InteropServices;

namespace DesktopSwitcher;

class HotKeyManager : IDisposable
{
    private readonly MessageWindow _messageWindow = new();

    public void Dispose()
    {
        _messageWindow.Dispose();
    }

    public int? RegisterHotKey(Modifiers modifiers, Keys keys, Action handler)
    {
        var id = _messageWindow.RegisterHandler(handler);

        if (!HotKeyBindings.RegisterHotKey(_messageWindow.Handle, id, (uint)modifiers, (uint)keys))
            return null;

        return id;
    }

    public bool UnregisterHotKey(int id)
    {
        _messageWindow.UnregisterHandler(id);

        return HotKeyBindings.UnregisterHotKey(_messageWindow.Handle, id);
    }
}

class MessageWindow : Form
{
    private static int _nextId = 9000;

    private readonly Dictionary<int, Action> _hotKeyHandlersById = new();

    public int RegisterHandler(Action handler)
    {
        var id = _nextId;
        _hotKeyHandlersById[id] = handler;
        _nextId++;

        return id;
    }

    public void UnregisterHandler(int id)
    {
        _hotKeyHandlersById.Remove(id);
    }

    protected override void WndProc(ref Message message)
    {
        const int WM_HOTKEY = 0x0312;
        if (message.Msg == WM_HOTKEY && _hotKeyHandlersById.TryGetValue(message.WParam.ToInt32(), out var handler))
        {
            try
            {
                handler.Invoke();
            }
            catch
            {
            }
        }

        base.WndProc(ref message);
    }
}
