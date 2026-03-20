namespace ProfileIpSwitcher.Services;

public interface IDialogService
{
    /// <summary>True = anwenden, False = abgebrochen. dontAskAgain setzt Einstellung.</summary>
    (bool apply, bool dontAskAgain) ConfirmApplyProfile(string profileName, bool skipIfConfigured);
}
