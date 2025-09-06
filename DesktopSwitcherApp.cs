namespace DesktopSwitcher;

using System.Windows.Forms;

public class DesktopSwitcherApp : Form
{
    private readonly NotifyIcon trayIcon;
    private readonly ContextMenuStrip trayMenu;

    public DesktopSwitcherApp()
    {
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Exit", null, (s, e) => ExitApp());

        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Desktop Switcher",
            Visible = true,
            ContextMenuStrip = trayMenu
        };

        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        Load += (s, e) => Hide();
    }

    private void ExitApp()
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        trayIcon.Visible = false;
        base.OnFormClosing(e);
    }
}
