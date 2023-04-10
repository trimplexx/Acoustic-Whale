using System;
using System.ComponentModel;
using System.Windows;

namespace SM_Audio_Player.View.UserControls.buttons;

public partial class ButtonMaximize : INotifyPropertyChanged
{
    private string? _maximizeIcon;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ButtonMaximize()
    {
        try
        {
            DataContext = this;
            MaximizeIcon = Icons.GetMaxIcon();
            InitializeComponent();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonMaximize Constructor exception: {ex.Message}");
            throw;
        }
    }

    /* PropertyChanged - pozwala na przekazywanie wartości do widoku */
    public string? MaximizeIcon
    {
        get => _maximizeIcon;
        set
        {
            _maximizeIcon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaximizeIcon"));
        }
    }

    /* Maxymalizacja okno playera */
    private void btnMaximize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.WindowState == WindowState.Normal)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                MaximizeIcon = Icons.GetMinIcon();
            }
            else if (Application.Current.MainWindow != null &&
                     Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                MaximizeIcon = Icons.GetMaxIcon();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ButtonMaximize click exception: {ex.Message}");
            throw;
        }
    }
}