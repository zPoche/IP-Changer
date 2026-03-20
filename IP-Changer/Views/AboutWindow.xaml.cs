using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace ProfileIpSwitcher.Views;

public partial class AboutWindow
{
    private AboutWindow()
    {
        InitializeComponent();
    }

    /// <summary>Zeigt den Info-Dialog modal; <paramref name="owner"/> für <see cref="WindowStartupLocation.CenterOwner"/>.</summary>
    public static void ShowDialog(Window? owner)
    {
        var w = new AboutWindow();
        if (owner != null)
        {
            w.Owner = owner;
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        w.ShowDialog();
    }

    private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "—";
        VersionTextBlock.Text = $"Version: {v}";
    }

    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/zPoche/IP-Changer",
            UseShellExecute = true
        });
        e.Handled = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
