using System.Windows;
using MahApps.Metro.Controls;

namespace ProfileIpSwitcher.Views;

public partial class ConfirmApplyWindow : MetroWindow
{
    public bool DoNotAskAgain { get; private set; }

    public ConfirmApplyWindow(string profileName)
    {
        InitializeComponent();
        MessageText.Text =
            $"Möchten Sie das Profil „{profileName}“ jetzt anwenden und die IP-Einstellungen des Adapters ändern?";
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DoNotAskAgain = DontAskCheckBox.IsChecked == true;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
