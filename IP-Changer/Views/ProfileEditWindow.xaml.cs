using System.Windows;
using MahApps.Metro.Controls;
using ProfileIpSwitcher.Models;
using ProfileIpSwitcher.ViewModels;

namespace ProfileIpSwitcher.Views;

public partial class ProfileEditWindow
{
    private readonly Func<NetworkAdapterInfo?>? _getLiveAdapter;

    private ProfileEditWindow(ProfileEditViewModel vm, Window owner, Func<NetworkAdapterInfo?>? getLiveAdapter)
    {
        InitializeComponent();
        Owner = owner;
        DataContext = vm;
        _getLiveAdapter = getLiveAdapter;
    }

    public static bool ShowDialog(ProfileEditViewModel vm, Window owner,
        Func<NetworkAdapterInfo?>? getLiveAdapter = null)
    {
        var w = new ProfileEditWindow(vm, owner, getLiveAdapter);
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

    private void LoadFromAdapter_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileEditViewModel vm) return;
        var a = _getLiveAdapter?.Invoke();
        vm.LoadFromAdapter(a);
    }
}
