namespace ProfileIpSwitcher.Services;

public interface IDialogService
{
    /// <summary>True = anwenden, False = abgebrochen. dontAskAgain setzt Einstellung.</summary>
    (bool apply, bool dontAskAgain) ConfirmApplyProfile(string profileName, bool skipIfConfigured);

    void ShowInformation(string message, string title = "Hinweis");

    void ShowWarning(string message, string title = "Hinweis");

    void ShowError(string message, string title = "Fehler");

    bool AskYesNo(string message, string title);
}
