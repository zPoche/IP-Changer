using System.Windows;
using MahApps.Metro.Controls;
using ProfileIpSwitcher.ViewModels;

namespace ProfileIpSwitcher.Views;

public partial class SettingsWindow
{
    private SettingsWindow(SettingsViewModel vm, Window owner)
    {
        InitializeComponent();
        Owner = owner;
        DataContext = vm;
    }

    public static bool ShowDialog(SettingsViewModel vm, Window owner)
    {
        var w = new SettingsWindow(vm, owner);
        return w.ShowDialog() == true;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
