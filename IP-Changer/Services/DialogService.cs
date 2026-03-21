using System.Windows;
using ProfileIpSwitcher.Views;

namespace ProfileIpSwitcher.Services;

public sealed class DialogService : IDialogService
{
    private readonly Window _owner;

    public DialogService(Window owner)
    {
        _owner = owner;
    }

    public (bool apply, bool dontAskAgain) ConfirmApplyProfile(string profileName, bool skipIfConfigured)
    {
        if (skipIfConfigured)
            return (true, false);

        var dlg = new ConfirmApplyWindow(profileName)
        {
            Owner = _owner
        };
        var result = dlg.ShowDialog();
        return (result == true, dlg.DoNotAskAgain);
    }

    public void ShowInformation(string message, string title = "Hinweis") =>
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowWarning(string message, string title = "Hinweis") =>
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowError(string message, string title = "Fehler") =>
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool AskYesNo(string message, string title) =>
        System.Windows.MessageBox.Show(_owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
        MessageBoxResult.Yes;
}
