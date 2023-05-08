using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SM_Audio_Player.View.UserControls.buttons;

///<summary>
/// Klasa reprezentująca przycisk do maksymalizacji okna odtwarzacza. Implementuje interfejs INotifyPropertyChanged.
///</summary>
public partial class ButtonMaximize : INotifyPropertyChanged
{
    private string? _maximizeIcon;

    public ButtonMaximize()
    {
        try
        {
            DataContext = this;
            MaximizeIcon = Icons.GetMaxIcon(); // Przypisanie ikony maksymalizacji do właściwości MaximizeIcon
            InitializeComponent();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonMaximize Constructor exception: {ex.Message}");
            throw;
        }
    }

    ///<summary>
    /// Właściwość reprezentująca ikonę przycisku maksymalizacji okna.
    ///</summary>
    public string? MaximizeIcon
    {
        get => _maximizeIcon;
        set
        {
            _maximizeIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaximizeIcon"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    ///<summary>
    /// Obsługuje kliknięcie przycisku maksymalizacji okna.
    ///</summary>
    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Znajdowanie elementu "world" na głównym oknie aplikacji
            var world = (Grid)Application.Current.MainWindow.FindName("World");
            if (Application.Current.MainWindow != null &&
                Application.Current.MainWindow.WindowState == WindowState.Normal)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized; // Maksymalizacja okna
                world.Margin = new Thickness(6); // Ustawienie marginesu elementu "world"
                MaximizeIcon = Icons.GetMinIcon(); // Zmiana ikony na minimalizację
            }
            else if (Application.Current.MainWindow != null &&
                     Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal; // Przywrócenie normalnego stanu okna
                world.Margin = new Thickness(0); // Usunięcie marginesu elementu "world"
                MaximizeIcon = Icons.GetMaxIcon(); // Zmiana ikony na maksymalizację
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonMaximize click exception: {ex.Message}");
            throw;
        }
    }
}