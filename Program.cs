namespace DesktopSwitcher;

using DesktopSwitcher.Bindings;

using System.Diagnostics;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        while (Process.GetProcessesByName("explorer").Length == 0)
            Thread.Sleep(500);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        while (WindowBindings.GetDesktopWindow() == IntPtr.Zero)
            Thread.Sleep(500);

        Thread.Sleep(2000);

        while (true)
        {
            try
            {
                using var desktopSwitchingService = new DesktopSwitchingService();
                desktopSwitchingService.RegisterHotKeys();

                Application.Run(new DesktopSwitcherApp());
            }
            catch
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
