namespace DesktopSwitcher;

using System.Windows.Forms;

public class DesktopSwitcherApp : Form
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;

    public DesktopSwitcherApp()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Exit", null, (s, e) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Desktop Switcher",
            Visible = true,
            ContextMenuStrip = _trayMenu
        };

        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        Load += (_, _) => Hide();
    }

    private void ExitApp()
    {
        _trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _trayIcon.Visible = false;

        base.OnFormClosing(e);
    }
}
