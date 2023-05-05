using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

/// <summary>
/// Klasa reprezentująca przycisk minimalizacji okna aplikacji.
/// </summary>
public partial class ButtonMinimize
{
    public ButtonMinimize()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia na przycisk minimalizacji.
    /// Minimalizuje okno aplikacji.
    /// </summary>
    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }
}