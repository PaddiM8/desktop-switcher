namespace DesktopSwitcher;

using System.Diagnostics;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var desktopSwitchingService = new DesktopSwitchingService();
        desktopSwitchingService.RegisterHotKeys();

        Application.Run(new DesktopSwitcherApp());
    }
}
