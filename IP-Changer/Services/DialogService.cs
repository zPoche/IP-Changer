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
}
